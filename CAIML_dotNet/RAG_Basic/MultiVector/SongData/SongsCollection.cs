using Helpers;
using LangChain.Databases;
using LangChain.DocumentLoaders;
using LangChain.Extensions;
using LangChain.Providers;

namespace MultiVector.SongData;

public class SongsCollection
{
    private const string CollectionName = "songs";

    private readonly IEmbeddingModel _embeddingModel;
    private readonly IVectorDatabase _vectorDatabase;

    public SongsCollection(
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

    public async Task LoadIntoDbCollection(IReadOnlyCollection<Document> files)
    {
        var songCollection = await _vectorDatabase.GetOrCreateCollectionAsync(
            CollectionName,
            OpenAiModelHelper.Dimensions);
        await songCollection.AddDocumentsAsync(_embeddingModel, files, EmbeddingSettings.Default);
    }

    public async Task<List<string>> RetrieveIds(string databaseFile)
    {
        return await VectorDbIds.ExtractFromCollection(databaseFile, CollectionName);
    }

    public async Task<Document> RetrieveSong(string id)
    {
        var collection = await _vectorDatabase.GetOrCreateCollectionAsync(
            CollectionName,
            OpenAiModelHelper.Dimensions);
        return await collection.GetDocumentByIdAsync(id) ?? Document.Empty;
    }
}