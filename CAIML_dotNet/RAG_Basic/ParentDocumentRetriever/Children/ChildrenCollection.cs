using Helpers;
using LangChain.Databases;
using LangChain.DocumentLoaders;
using LangChain.Extensions;
using LangChain.Providers;
using LangChain.Splitters.Text;

namespace ParentDocumentRetriever.Children;

public class ChildrenCollection
{
    private const string CollectionName = "children";

    private readonly IEmbeddingModel _embeddingModel;
    private readonly IVectorDatabase _vectorDatabase;

    public ChildrenCollection(
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
    
    public async Task LoadIntoCollectionAsync(List<(string, string)> idAndText)
    {
        var childCollection = await _vectorDatabase.GetOrCreateCollectionAsync(
            CollectionName, 
            OpenAiModelHelper.Dimensions);
        
        var splitter = new CharacterTextSplitter(" ", 200, 0);
        var documents = idAndText.Select(tuple => splitter
                .CreateDocuments([tuple.Item2], [new Dictionary<string, object> { { "parentId", tuple.Item1 } }]))
            .Aggregate(new List<Document>(), (list, onlyList) => list.Concat(onlyList).ToList());
        
        await childCollection.AddDocumentsAsync(_embeddingModel, documents);
    }
    
    public async Task<IReadOnlyCollection<Document>> Retrieve(string question)
    {
        var collection = await _vectorDatabase.GetOrCreateCollectionAsync(
            CollectionName,
            OpenAiModelHelper.Dimensions);
        return await collection.GetSimilarDocuments(_embeddingModel, question, 2);
    }
}