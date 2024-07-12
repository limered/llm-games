﻿using LangChain.Providers;
using LangChain.Providers.OpenAI;
using OpenAI.Constants;

namespace Helpers;

public static class OpenAiModelHelper
{
    public static OpenAiChatModel SetupLLM()
    {
        var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY")!;
        var llmModel = new OpenAiChatModel(apiKey, ChatModels.Gpt35Turbo);
        llmModel.PromptSent += (_, s) =>
        {
            Console.WriteLine(
                "---------------------------------------------------------------------------------------------------");
            Console.WriteLine("Prompt:");
            Console.Write(s);
        };
        llmModel.PartialResponseGenerated += (_, s) => { Console.Write(s); };

        return llmModel;
    }

    public static IEmbeddingModel SetupEmbedding()
    {
        var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY")!;
        return new OpenAiEmbeddingModel(apiKey, "text-embedding-ada-002");
    }
}