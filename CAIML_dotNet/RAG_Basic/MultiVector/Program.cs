using Helpers;
using LangChain.Chains;
using LangChain.Databases.Sqlite;
using LangChain.DocumentLoaders;
using MultiVector;
using MultiVector.HypotheticalQuestions;
using MultiVector.SongData;
using MultiVector.Summaries;

const bool indexSongs = false;
const bool indexQuestions = false;
const bool indexSummaries = false;

var llm = OpenAiModelHelper.SetupLLM(false);
var embeddingModel = OpenAiModelHelper.SetupEmbedding();

const string databaseFile = "vectors.db";
using var vectorDatabase = new SqLiteVectorDatabase(databaseFile);

var songs = new SongsCollection(embeddingModel, vectorDatabase);
var questionCollection = new QuestionsCollection(embeddingModel, vectorDatabase);
var summaryCollection = new SummaryCollection(embeddingModel, vectorDatabase, llm);

if (indexSongs) await songs.ClearCollection();
if (indexQuestions) await questionCollection.ClearCollection();
if (indexSummaries) await summaryCollection.ClearCollection();

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

if (indexSongs) await songs.LoadIntoDbCollection(files);

var songIds = await songs.RetrieveIds(databaseFile);

if (indexQuestions) await questionCollection.LoadIntoDbCollection(files, songIds);

if (indexSummaries) await summaryCollection.LoadIntoDbCollection(files, songIds);


// const string question = "Everybody expects too much of me. I'm tired of it. I need to be free. What should I do?";
// const string question = "Can i get free tickets to the concert?";
// const string question = "Is everything my fault?";
// const string question = "Song about importance of self-worth and independence in a relationship.";
const string question = "I struggle with self-perception. What can i do?";

var relevantQuestions = await questionCollection.Retrieve(question);
var relevantSummaries = await summaryCollection.Retrieve(question);

var reranker = new SimpleReranker(embeddingModel);
var bestSongId = await reranker.Rerank(relevantQuestions.Concat(relevantSummaries).ToArray(), question);
var bestSongDoc = await songs.RetrieveSong(bestSongId);
var context = bestSongDoc.PageContent;

const string prompt =
    """
    You are Taylor Swift.
    A person, who seeks emotional guidance asks for your help.
    Tell this person exactly what he or she needs to do to resolve his / her issues.
    Do mention your song's title and that listening to it will help the person.
    Use a passage from the song to support your advice.
    Answer the Question only using the context you are provided with.:

    Context:
    {context}

    Question:
    {question}
    """;

var chain =
    Chain.Set(context, "context") |
    Chain.Set(question, "question") |
    Chain.Template(prompt) |
    Chain.LLM(llm, "text", "result");

var answer = await chain.RunAsync();

Console.WriteLine(answer.Value["result"].ToString());