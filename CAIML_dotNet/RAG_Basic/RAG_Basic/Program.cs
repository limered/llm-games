using Helpers;
using LangChain.Chains;
using LangChain.DocumentLoaders;
using LangChain.Splitters.Text;
using LangChain.Databases.Sqlite;
using LangChain.Extensions;
using LangChain.Providers;

// load model
var llmModel = OpenAiModelHelper.SetupLLM();
var embeddingModel = OpenAiModelHelper.SetupEmbedding();

// Data
var pathToFile = Directory.GetParent(Environment.CurrentDirectory)!.Parent!.Parent!.FullName
    .Split(Path.DirectorySeparatorChar).Append("apple.txt").ToArray();
// var files = await new FileLoader().LoadAsync(DataSource.FromPath(Path.Combine(pathToFile)));
var textSplitter = new RecursiveCharacterTextSplitter(chunkSize: 500, chunkOverlap: 0);

// Setup DB
const string databaseFile = "vectors.db";
const string databaseTable = "apple";
var vectorDatabase = new SqLiteVectorDatabase(databaseFile);
var vectorCollection = await vectorDatabase.AddDocumentsFromAsync<FileLoader>(
    embeddingModel: embeddingModel,
    dimensions: 1536,
    dataSource: DataSource.FromPath(Path.Combine(pathToFile)),
    collectionName: databaseTable,
    textSplitter: textSplitter,
    loaderSettings: new DocumentLoaderSettings { ShouldCollectMetadata = true },
    embeddingSettings: EmbeddingSettings.Default,
    behavior: AddDocumentsToDatabaseBehavior.OverwriteExistingCollection
);

// query
const string question =
    "How did Timmy and Martina manage to make the Apple computer both easy to use and powerful with just old parts?";
var similarDocuments = await vectorCollection.GetSimilarDocuments(
    embeddingModel: embeddingModel,
    request: question,
    amount: 2);

Console.WriteLine(similarDocuments.AsString());

// building a chain
var prompt = $"""
              Use the following pieces of context to answer the question at the end.
              If the answer is not in context then just say that you don't know, don't try to make up an answer.
              Keep the answer short.
              
              << Context >>
              {similarDocuments.AsString()}
              << Context End >>
              
              Question: {question}
              
              Helpful Answer:
              
              """;

var chain = 
    Chain.Set(prompt, outputKey:"prompt")
    | Chain.LLM(llmModel, inputKey:"prompt");

chain.RunAsync().Wait();