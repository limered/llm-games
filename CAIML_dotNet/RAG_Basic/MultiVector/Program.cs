using Helpers;
using LangChain.Databases.Sqlite;
using LangChain.DocumentLoaders;
using LangChain.Extensions;
using MultiVector.HypotheticalQuestions;
using MultiVector.SongData;

const bool indexSongs = true;
const bool indexQuestions = false;
const bool indexSummaries = true;

var llmModel = OpenAiModelHelper.SetupLLM(false);
var embeddingModel = OpenAiModelHelper.SetupEmbedding();

const string databaseFile = "vectors.db";
const string summaryTable = "summaries";
const string questionsTable = "Questions";
var vectorDatabase = new SqLiteVectorDatabase(databaseFile);

var songs = new SongsCollection(embeddingModel, vectorDatabase);

if (indexSongs)
{
    await songs.ClearCollection();
}
if (indexQuestions)
{
    await vectorDatabase.DeleteCollectionAsync(questionsTable);
}
if (indexSummaries)
{
    await vectorDatabase.DeleteCollectionAsync(summaryTable);
}

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
    await songs.LoadIntoDbCollection(files);
}

var songIds = await songs.RetrieveIds(databaseFile);

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