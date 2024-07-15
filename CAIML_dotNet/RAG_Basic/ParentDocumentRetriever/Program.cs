using System.Text;
using Helpers;
using LangChain.Chains;
using LangChain.Databases.Sqlite;
using LangChain.DocumentLoaders;
using LangChain.Extensions;
using ParentDocumentRetriever.Children;
using ParentDocumentRetriever.Parents;



const bool indexParents = true;
const bool indexChildren = true;

const RetrievalMode retrievalMode = RetrievalMode.OnlyChild;

var llmModel = OpenAiModelHelper.SetupLLM();
var embeddingModel = OpenAiModelHelper.SetupEmbedding();

const string databaseFile = "vectors.db";
var vectorDatabase = new SqLiteVectorDatabase(databaseFile);

var parentCollection = new ParentsCollection(embeddingModel, vectorDatabase);
var childCollection = new ChildrenCollection(embeddingModel, vectorDatabase);

if (indexParents) await parentCollection.ClearCollection();
if (indexChildren) await childCollection.ClearCollection();

var pathToFile = Path.Combine(
    Directory.GetParent(Environment.CurrentDirectory)!.Parent!.Parent!.FullName
        .Split(Path.DirectorySeparatorChar).Append("turm.txt").ToArray()
);
var documents = await new FileLoader().LoadAsync(DataSource.FromPath(pathToFile));

if (indexParents) await parentCollection.LoadIntoCollectionAsync(documents.ToArray());

var parentIdToText = await parentCollection.ExtractIdAndTextOfDocuments(databaseFile);
if (indexChildren) await childCollection.LoadIntoCollectionAsync(parentIdToText);

// const string question = "Tell me about unusual incidents associated with the tower?";
const string question = "Give me the most important information for the tower?";

var similarDocuments = retrievalMode == RetrievalMode.OnlyParent
    ? await parentCollection.RetrieveSimilar(question)
    : await childCollection.Retrieve(question);

var result = similarDocuments.AsString();

if (retrievalMode == RetrievalMode.Mixed && similarDocuments.Count != 0)
{
    var sb = new StringBuilder();
    foreach (var document in similarDocuments)
    {
        var parentDocumentText = await parentCollection.RetrieveDocument(document.Metadata["parentId"].ToString()!);
        sb.AppendLine(parentDocumentText.PageContent);
    }

    result = sb.ToString();
}

Console.WriteLine("Child: ");
Console.WriteLine(similarDocuments.AsString());
Console.WriteLine("Parent: ");
Console.WriteLine(result);

// building a chain
const string prompt =
    """
    Use only the following pieces of context to answer the question at the end.
    If the answer is not in context then just say that you don't know, don't try to make up an answer.
    Give as much detail as possible without inventing or using learned knowledge.
    Be verbose!

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

public enum RetrievalMode
{
    OnlyChild,
    OnlyParent,
    Mixed
}