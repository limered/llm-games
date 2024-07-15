using Helpers;
using LangChain.Databases.Sqlite;
using LangChain.DocumentLoaders;
using LangChain.Extensions;
using LangChain.Providers;
using MultiVector;

const bool indexSongs = false;
const bool indexQuestions = false;
const bool indexSummaries = true;

var llmModel = OpenAiModelHelper.SetupLLM(false);
var embeddingModel = OpenAiModelHelper.SetupEmbedding();

const string databaseFile = "vectors.db";
const string songTable = "songs";
const string summaryTable = "summaries";
const string questionsTable = "Questions";
var vectorDatabase = new SqLiteVectorDatabase(databaseFile);

if (indexSongs)
{
    await vectorDatabase.DeleteCollectionAsync(songTable);
}
if (indexQuestions)
{
    await vectorDatabase.DeleteCollectionAsync(questionsTable);
}
if (indexSummaries)
{
    await vectorDatabase.DeleteCollectionAsync(summaryTable);
}

var songCollection = await vectorDatabase.GetOrCreateCollectionAsync(songTable, OpenAiModelHelper.Dimensions);
var summaryCollection = await vectorDatabase.GetOrCreateCollectionAsync(summaryTable, OpenAiModelHelper.Dimensions);
var questionsCollection = await vectorDatabase.GetOrCreateCollectionAsync(questionsTable, OpenAiModelHelper.Dimensions);

string[] fileNames =
[
    "anti_hero",
    "bejewled",
    "lavender_haze",
    "maroon",
    "snow_on_the_beach"
];
var pathToFolder =
    Directory.GetParent(Environment.CurrentDirectory)!.Parent!.Parent!.FullName
        .Split(Path.DirectorySeparatorChar).Append<string>("lyrics").ToArray();
var fileTasks = fileNames
    .Select(name => Path.Combine(pathToFolder.Append(name + ".txt").ToArray()))
    .Select(path => new FileLoader().LoadAsync(DataSource.FromPath(path)));

var files = (await Task.WhenAll(fileTasks)).Select(documents => documents.First()).ToArray();

if (indexSongs)
{
    await songCollection.AddDocumentsAsync(embeddingModel, files, EmbeddingSettings.Default);
}

var songIds = await VectorDbIds.ExtractFromCollection(databaseFile, songTable);

if (indexQuestions)
{
    const int songNr = 0;
    
    var questionGenerator = new Questions();
    var questionDocuments = new List<Document>();

    var questions = await questionGenerator.GenerateForAsync(files[songNr].PageContent, 3);
    questionDocuments.AddRange(questions.Select(question =>
        new Document(question, new Dictionary<string, object> { { "parentId", songIds[songNr] } })));

    await questionsCollection.AddDocumentsAsync(embeddingModel, questionDocuments);
}

if (indexSummaries)
{
    
}

// RAG