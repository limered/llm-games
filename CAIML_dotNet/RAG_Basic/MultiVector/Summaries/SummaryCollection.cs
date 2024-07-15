using Helpers;
using LangChain.Databases;
using LangChain.DocumentLoaders;
using LangChain.Extensions;
using LangChain.Providers;
using LangChain.Providers.OpenAI;

namespace MultiVector.Summaries;

public class SummaryCollection
{
    private const string CollectionName = "summaries";

    private readonly IEmbeddingModel _embeddingModel;
    private readonly OpenAiChatModel _llm;
    private readonly IVectorDatabase _vectorDatabase;

    public SummaryCollection(
        IEmbeddingModel embeddingModel,
        IVectorDatabase vectorDatabase,
        OpenAiChatModel llm)
    {
        _embeddingModel = embeddingModel;
        _vectorDatabase = vectorDatabase;
        _llm = llm;
    }

    public async Task ClearCollection()
    {
        await _vectorDatabase.DeleteCollectionAsync(CollectionName);
    }

    public async Task LoadIntoDbCollection(Document[] files, List<string> songIds)
    {
        var summaryCollection = await _vectorDatabase.GetOrCreateCollectionAsync(
            CollectionName,
            OpenAiModelHelper.Dimensions);

        var summeryGenerator = new SummaryGenerator(_llm);

        var summaryTasks = files
            .Select((file, i) => (file, songId: songIds[i]))
            .Select(t => summeryGenerator.GenerateForTextAsync(t.file.PageContent, t.songId));

        var summaries = await Task.WhenAll(summaryTasks);

        await summaryCollection.AddDocumentsAsync(_embeddingModel, summaries, EmbeddingSettings.Default);
    }

    public async Task<IReadOnlyCollection<Document>> Retrieve(string text)
    {
        var summaryCollection = await _vectorDatabase.GetOrCreateCollectionAsync(
            CollectionName,
            OpenAiModelHelper.Dimensions);
        return await summaryCollection.GetSimilarDocuments(_embeddingModel, text, 2);
    }
}