using System.Text.Json;
using System.Text.Json.Nodes;
using Helpers;
using LangChain.Chains;
using LangChain.Providers.OpenAI;
using OpenAI;

namespace MultiVector;

public class Questions
{
    private const string QuestionsPrompt =
        """
        Your goal is to generate a list of exactly {count} hypothetical questions.

        You get sent song lyrics.
        Generate a list of exactly {count} hypothetical questions that a person, 
        who seeks emotional guidance would ask that could be answered by this song's lyrics and or meaning. 
        Do not mention the song or the lyrics in these questions.
        Do not add any counter to these questions.

        The lyrics:
        {context}

        Generated questions:
        """;

    private readonly OpenAiChatModel _llm = OpenAiModelHelper.SetupLLM(false);

    private readonly Tool _questionTool = new Function(
        "hypothetical_questions",
        "Generate hypothetical questions",
        new JsonObject([
            new KeyValuePair<string, JsonNode?>("type", "object"),
            new KeyValuePair<string, JsonNode?>("properties", new JsonObject([
                new KeyValuePair<string, JsonNode?>("Questions", new JsonObject([
                    new KeyValuePair<string, JsonNode?>("type", "array"),
                    new KeyValuePair<string, JsonNode?>("items",
                        new JsonObject([new KeyValuePair<string, JsonNode?>("type", "string")]))
                ]))
            ])),
            new KeyValuePair<string, JsonNode?>("required", new JsonArray("Questions"))
        ]));

    private ResultingQuestions? _generatedTexts;

    public Questions()
    {
        _llm.AddGlobalTools([_questionTool], new Dictionary<string, Func<string, CancellationToken, Task<string>>>
        {
            {
                "hypothetical_questions", (s, _) =>
                {
                    _generatedTexts = JsonSerializer.Deserialize<ResultingQuestions>(s);
                    return Task.FromResult(s);
                }
            }
        });
    }

    public async Task<string[]?> GenerateForAsync(string text, int questionCount)
    {
        _generatedTexts = null;

        var questionChain =
            Chain.Set(text, "context")
            | Chain.Set(questionCount, "count")
            | Chain.Template(QuestionsPrompt)
            | Chain.LLM(_llm);

        await questionChain.RunAsync();

        return _generatedTexts?.Questions;
    }
}

public class ResultingQuestions
{
    public string[]? Questions { get; init; }
}