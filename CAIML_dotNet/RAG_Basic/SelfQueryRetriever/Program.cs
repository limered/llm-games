using Helpers;
using LangChain.Databases;
using LangChain.Databases.Sqlite;
using LangChain.DocumentLoaders;
using LangChain.Extensions;
using LangChain.Providers;
using SelfQueryRetriever;

// load model
var llmModel = OpenAiModelHelper.SetupLLM();
var embeddingModel = OpenAiModelHelper.SetupEmbedding();

// Data
var docs = Library.Books();

const string databaseFile = "vectors.db";
const string databaseTable = "books";
var vectorDatabase = new SqLiteVectorDatabase(databaseFile);
var collection = await vectorDatabase.GetOrCreateCollectionAsync(databaseTable, 1536);
if(await collection.IsEmptyAsync())
{
    await collection.AddDocumentsAsync(embeddingModel, docs, EmbeddingSettings.Default);
}


