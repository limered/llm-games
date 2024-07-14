using Helpers;
using LangChain.Chains.StackableChains.Agents.Tools;
using LangChain.Databases.Sqlite;
using LangChain.DocumentLoaders;
using LangChain.Extensions;
using LangChain.Providers;
using MultiVector;

const bool indexData = true;
const bool retrieveParent = true;
const bool useOnlyParent = false;

var llmModel = OpenAiModelHelper.SetupLLM();
var embeddingModel = OpenAiModelHelper.SetupEmbedding();

const string databaseFile = "vectors.db";
const string songTable = "songs";
const string summaryTable = "summaries";
const string questionsTable = "Questions";
var vectorDatabase = new SqLiteVectorDatabase(databaseFile);
if (indexData)
{
    await vectorDatabase.DeleteCollectionAsync(songTable);
    await vectorDatabase.DeleteCollectionAsync(summaryTable);
    await vectorDatabase.DeleteCollectionAsync(questionsTable);
}

var songCollection = await vectorDatabase.GetOrCreateCollectionAsync(songTable, OpenAiModelHelper.Dimensions);
var summaryCollection = await vectorDatabase.GetOrCreateCollectionAsync(summaryTable, OpenAiModelHelper.Dimensions);
var questionsCollection = await vectorDatabase.GetOrCreateCollectionAsync(questionsTable, OpenAiModelHelper.Dimensions);

if (indexData)
{
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

    await songCollection.AddDocumentsAsync(embeddingModel, files, EmbeddingSettings.Default);

    var songIds = VectorDbIds.ExtractFromCollection(databaseFile, songTable);
    
    // var tool = new AgentToolLambda()
}