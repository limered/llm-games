using Helpers;
using LangChain.Chains;
using LangChain.Databases.Sqlite;
using LangChain.DocumentLoaders;
using LangChain.Extensions;
using LangChain.Providers;
using LangChain.Splitters.Text;

var llmModel = OpenAiModelHelper.SetupLLM();
var embeddingModel = OpenAiModelHelper.SetupEmbedding();

var pathToFile = Directory.GetParent(Environment.CurrentDirectory)!.Parent!.Parent!.FullName
    .Split(Path.DirectorySeparatorChar).Append("apple.txt").ToArray();

var textSplitter = new RecursiveCharacterTextSplitter(chunkSize: 500, chunkOverlap: 0);

// Setup DB
const string databaseFile = "vectors.db";
const string databaseTable = "apple";
var vectorDatabase = new SqLiteVectorDatabase(databaseFile);

var vectorCollection = await vectorDatabase.AddDocumentsFromAsync<FileLoader>(
    embeddingModel,
    OpenAiModelHelper.Dimensions,
    DataSource.FromPath(Path.Combine(pathToFile)),
    databaseTable,
    textSplitter,
    new DocumentLoaderSettings { ShouldCollectMetadata = true },
    EmbeddingSettings.Default,
    AddDocumentsToDatabaseBehavior.OverwriteExistingCollection
);

// query
const string question =
    "How did Timmy and Martina manage to make the Apple computer both easy to use and powerful with just old parts?";

var similarDocuments = await vectorCollection.GetSimilarDocuments(
    embeddingModel,
    question,
    2);

Console.WriteLine(similarDocuments.AsString());

// building a chain
var prompt =
    """
    Use the following pieces of context to answer the question at the end.
    If the answer is not in context then just say that you don't know, don't try to make up an answer.
    Keep the answer short.

    << Context >>
    {context}
    << Context End >>

    Question: {question}

    Helpful Answer:

    """;

var chain =
    Chain.Set(similarDocuments.AsString(), "context") |
    Chain.Set(question, "question") |
    Chain.Template(prompt) |
    Chain.LLM(llmModel);

chain.RunAsync().Wait();