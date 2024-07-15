using LangChain.DocumentLoaders;
using LangChain.Providers;
using Utils = LangChain.Databases.Utils;

namespace MultiVector;

public class SimpleReranker
{
    private readonly IEmbeddingModel _embeddingModel;

    public SimpleReranker(IEmbeddingModel embeddingModel)
    {
        _embeddingModel = embeddingModel;
    }

    public async Task<string> Rerank(Document[] documents, string question)
    {
        var questionEmbedding = await _embeddingModel.CreateEmbeddingsAsync(question);
        
        var embeddingTasks = documents
            .Select(doc => _embeddingModel.CreateEmbeddingsAsync(doc.PageContent));

        var embedding = (await Task.WhenAll(embeddingTasks))
            .Select((embedding, i) => (
                dist: Utils.ComputeEuclideanDistance(embedding.ToSingleArray(), questionEmbedding.ToSingleArray()),
                embedding, 
                songId: (string)documents[i].Metadata["songId"])).MinBy(t => t.dist);

        return embedding.songId;
    }
}