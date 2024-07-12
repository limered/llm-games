using System.Text.Json;
using Helpers;
using LangChain.Chains;
using LangChain.Databases.Sqlite;
using LangChain.DocumentLoaders;
using LangChain.Extensions;
using LangChain.Splitters.Text;
using Microsoft.Data.Sqlite;

var llmModel = OpenAiModelHelper.SetupLLM();
var embeddingModel = OpenAiModelHelper.SetupEmbedding();

const string databaseFile = "vectors.db";
const string parentTable = "parents";
const string childTable = "children";
var vectorDatabase = new SqLiteVectorDatabase(databaseFile);

var parentCollection = await vectorDatabase.GetOrCreateCollectionAsync(parentTable, OpenAiModelHelper.Dimensions);
var childCollection = await vectorDatabase.GetOrCreateCollectionAsync(childTable, OpenAiModelHelper.Dimensions);

// Data
var pathToFile = Path.Combine(
    Directory.GetParent(Environment.CurrentDirectory)!.Parent!.Parent!.FullName
        .Split(Path.DirectorySeparatorChar).Append("tesla.txt").ToArray()
);

var documents = await new FileLoader().LoadAsync(DataSource.FromPath(pathToFile));
var parentSplitter = new CharacterTextSplitter(separator: " ", chunkSize: 2000, chunkOverlap: 0);
var childSplitter = new CharacterTextSplitter(separator: " ", chunkSize: 200, chunkOverlap: 0);

var splitParentDocuments = parentSplitter.SplitDocuments(documents);

Console.WriteLine($"Parent Document Count: {splitParentDocuments.Count}");

await parentCollection.AddDocumentsAsync(embeddingModel, splitParentDocuments);

// Read Back Data
await using var connection = new SqliteConnection($"Data Source={databaseFile}");
connection.Open();

var command = connection.CreateCommand();
command.CommandText = $"SELECT * FROM {parentCollection.Name}";

var reader = await command.ExecuteReaderAsync().ConfigureAwait(false);

var retrievedParentVectors = new List<(string, string)>();
while (await reader.ReadAsync().ConfigureAwait(false))
{
    var id = reader.GetString(0);
    var doc = await reader.GetFieldValueAsync<string>(2).ConfigureAwait(false);
    var document = JsonSerializer.Deserialize<Document>(doc) ?? Document.Empty;
    var text = document.PageContent;
    
    retrievedParentVectors.Add((id, text));
}

connection.Close();

var splitChildDocuments = retrievedParentVectors
    .Select(tuple => childSplitter.CreateDocuments([tuple.Item2], [new Dictionary<string, object>{ {"parentId", tuple.Item1} }]))
    .Aggregate(new List<Document>(), (list, onlyList) => list.Concat(onlyList).ToList());

Console.WriteLine($"Child Document Count: {splitChildDocuments.Count}");

await childCollection.AddDocumentsAsync(embeddingModel, splitChildDocuments);

///////////
//  RAG  //
///////////

// query
const string question =
    "What was the reason for Tesla not to mary?";
var similarDocuments = await childCollection.GetSimilarDocuments(
    embeddingModel: embeddingModel,
    request: question,
    amount: 1);

var result = similarDocuments.AsString();

if(similarDocuments.Count != 0)
{
    result = (await parentCollection.GetAsync(similarDocuments.First().Metadata["parentId"].ToString()!))!.Text;
}

Console.WriteLine("Child: ");
Console.WriteLine(similarDocuments.AsString());
Console.WriteLine("Parent: ");
Console.WriteLine(result);

// building a chain
var prompt = $"""
              Use only the following pieces of context to answer the question at the end.
              If the answer is not in context then just say that you don't know, don't try to make up an answer.

              << Context >>
              {result}
              << Context End >>

              Question: {question}

              Helpful Answer:

              """;

var chain = 
    Chain.Set(prompt, outputKey:"prompt")
    | Chain.LLM(llmModel, inputKey:"prompt");

chain.RunAsync().Wait();