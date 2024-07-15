using Helpers;
using LangChain.Databases;
using LangChain.DocumentLoaders;
using LangChain.Extensions;
using LangChain.Providers;

namespace MultiVector.HypotheticalQuestions;

public class QuestionsCollection
{
    private const string CollectionName = "questions";

    private readonly IEmbeddingModel _embeddingModel;
    private readonly IVectorDatabase _vectorDatabase;

    public QuestionsCollection(
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

    public async Task LoadIntoDbCollection(Document[] files, List<string> songIds)
    {
        const int songNr = 0;

        var questionCollection = await _vectorDatabase.GetOrCreateCollectionAsync(
            CollectionName,
            OpenAiModelHelper.Dimensions);

        var questionGenerator = new QuestionsGenerator();

        var questions = await questionGenerator.GenerateForAsync(files[songNr].PageContent, 3);
        var questionDocuments = questions
            .Select(question => new Document(
                question, new Dictionary<string, object> { { "songId", songIds[songNr] } }))
            .ToList();

        await questionCollection.AddDocumentsAsync(_embeddingModel, questionDocuments, EmbeddingSettings.Default);
    }

    public async Task<IReadOnlyCollection<Document>> Retrieve(string question)
    {
        var questionCollection = await _vectorDatabase.GetOrCreateCollectionAsync(
            CollectionName,
            OpenAiModelHelper.Dimensions);
        return await questionCollection.GetSimilarDocuments(_embeddingModel, question, 2);
    }
}