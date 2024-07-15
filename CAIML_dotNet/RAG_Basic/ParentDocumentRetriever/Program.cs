using System.Text;
using Helpers;
using LangChain.Chains;
using LangChain.Databases.Sqlite;
using LangChain.DocumentLoaders;
using LangChain.Extensions;
using LangChain.Splitters.Text;
using ParentDocumentRetriever;

const bool indexData = false;
const bool retrieveParent = true;
const bool useOnlyParent = false;

var llmModel = OpenAiModelHelper.SetupLLM();
var embeddingModel = OpenAiModelHelper.SetupEmbedding();

const string databaseFile = "vectors.db";
const string parentTable = "parents";
const string childTable = "children";
var vectorDatabase = new SqLiteVectorDatabase(databaseFile);
if (indexData)
{
    await vectorDatabase.DeleteCollectionAsync(parentTable);
    await vectorDatabase.DeleteCollectionAsync(childTable);
}

var parentCollection = await vectorDatabase.GetOrCreateCollectionAsync(parentTable, OpenAiModelHelper.Dimensions);
var childCollection = await vectorDatabase.GetOrCreateCollectionAsync(childTable, OpenAiModelHelper.Dimensions);

// Data
if (indexData)
{
    var pathToFile = Path.Combine(
        Directory.GetParent(Environment.CurrentDirectory)!.Parent!.Parent!.FullName
            .Split(Path.DirectorySeparatorChar).Append("turm.txt").ToArray()
    );
    var documents = await new FileLoader().LoadAsync(DataSource.FromPath(pathToFile));

    var parentSplitter = new CharacterTextSplitter(" ", 5000, 0);
    var childSplitter = new CharacterTextSplitter(" ", 500, 0);

    var splitParentDocuments = parentSplitter.SplitDocuments(documents);
    await parentCollection.AddDocumentsAsync(embeddingModel, splitParentDocuments);

// Read Back Data
    var splitChildDocuments = await ChildDocuments
        .ExtractFromParentCollection(databaseFile, parentCollection.Name, childSplitter);
    await childCollection.AddDocumentsAsync(embeddingModel, splitChildDocuments);

    Console.WriteLine($"Parent Document Count: {splitParentDocuments.Count}");
    Console.WriteLine($"Child Document Count: {splitChildDocuments.Count}");
}

///////////
//  RAG  //
///////////

// query

const string question = "What lights did the tower have?";
IReadOnlyCollection<Document>? similarDocuments;
if (useOnlyParent)
    similarDocuments = await parentCollection.GetSimilarDocuments(
        embeddingModel,
        question,
        1);
similarDocuments = await childCollection.GetSimilarDocuments(
    embeddingModel,
    question,
    2);

var result = similarDocuments.AsString();

if (retrieveParent && similarDocuments.Count != 0)
{
    var sb = new StringBuilder();
    foreach (var document in similarDocuments)
    {
        var parentDocumentText = (await parentCollection.GetAsync(document.Metadata["parentId"].ToString()!))!.Text;
        sb.AppendLine(parentDocumentText);
    }

    result = sb.ToString();
}

Console.WriteLine("Child: ");
Console.WriteLine(similarDocuments.AsString());
Console.WriteLine("Parent: ");
Console.WriteLine(result);

// building a chain
var prompt =
    """
    Use only the following pieces of context to answer the question at the end.
    If the answer is not in context then just say that you don't know, don't try to make up an answer.
    Give as much detail as possible without inventing or using learned knowledge.

    << Context >>
    {context}
    << Context End >>

    Question: {question}

    Helpful Answer:

    """;

var chain =
    Chain.Set(result, "context") |
    Chain.Set(question, "question") |
    Chain.Template(prompt) |
    Chain.LLM(llmModel);

chain.RunAsync().Wait();