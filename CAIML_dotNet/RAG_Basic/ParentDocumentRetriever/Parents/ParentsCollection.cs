using System.Text.Json;
using Helpers;
using LangChain.Databases;
using LangChain.DocumentLoaders;
using LangChain.Extensions;
using LangChain.Providers;
using LangChain.Splitters.Text;
using Microsoft.Data.Sqlite;

namespace ParentDocumentRetriever.Parents;

public class ParentsCollection
{
    private const string CollectionName = "parents";
    
    private readonly IEmbeddingModel _embeddingModel;
    private readonly IVectorDatabase _vectorDatabase;
    
    public ParentsCollection(
        IEmbeddingModel embeddingModel,
        IVectorDatabase vectorDatabase)
    {
        _embeddingModel = embeddingModel;
        _vectorDatabase = vectorDatabase;
    }
    
    public async Task ClearCollection()
    {
        await _vectorDatabase.DeleteCollectionAsync(CollectionName);
    }

    public async Task LoadIntoCollectionAsync(Document[] files)
    {
        var parentCollection = await _vectorDatabase.GetOrCreateCollectionAsync(
            CollectionName, 
            OpenAiModelHelper.Dimensions);
        
        var splitter = new CharacterTextSplitter(" ", 5000, 0);

        var splitParentDocuments = splitter.SplitDocuments(files);
        await parentCollection.AddDocumentsAsync(_embeddingModel, splitParentDocuments);
    }

    public async Task<List<(string, string)>> ExtractIdAndTextOfDocuments(string dbName)
    {
        await using var connection = new SqliteConnection($"Data Source={dbName}");
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = $"SELECT * FROM {CollectionName}";

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

        return retrievedParentVectors;
    }
    
    public async Task<IReadOnlyCollection<Document>> RetrieveSimilar(string question)
    {
        var collection = await _vectorDatabase.GetOrCreateCollectionAsync(
            CollectionName,
            OpenAiModelHelper.Dimensions);
        return await collection.GetSimilarDocuments(_embeddingModel, question, 2);
    }
    
    public async Task<Document> RetrieveDocument(string id)
    {
        var collection = await _vectorDatabase.GetOrCreateCollectionAsync(
            CollectionName,
            OpenAiModelHelper.Dimensions);
        return await collection.GetDocumentByIdAsync(id) ?? Document.Empty;
    }
}