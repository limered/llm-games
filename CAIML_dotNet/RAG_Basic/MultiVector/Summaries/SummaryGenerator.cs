using LangChain.Chains;
using LangChain.DocumentLoaders;
using LangChain.Providers.OpenAI;

namespace MultiVector.Summaries;

public class SummaryGenerator
{
    private const string Prompt =
        """
        Your goal is to generate a summry of the song i provide.

        Summarize the following document. Do not include the title. Do not mention the Document.

        The document:
        {context}

        Generated summary:
        """;

    private readonly OpenAiChatModel _llm;

    public SummaryGenerator(OpenAiChatModel llm)
    {
        _llm = llm;
    }

    public async Task<Document> GenerateForTextAsync(string text, string songId)
    {
        var chain =
            Chain.Set(text, "context") |
            Chain.Template(Prompt) |
            Chain.LLM(_llm);

        var summary = await chain.RunAsync<string>("text");
        return new Document(summary, new Dictionary<string, object> { { "songId", songId } });
    }
}