namespace OcrOrchestratorApi.BusinessLogic
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Azure;
    using Azure.AI.OpenAI;
    using OpenAI.Chat;
    public class OpenAiOrchestrator
    {
        private readonly AzureOpenAIClient _azureOpenAIClient;
        private readonly string _deployment;


        public OpenAiOrchestrator(string endpointUrl, string apiKey, string deployment)
        {
            _azureOpenAIClient = new AzureOpenAIClient(new Uri(endpointUrl), new AzureKeyCredential(apiKey));
            
            _deployment = deployment;
        }

        /// <summary>
        /// Sends a full conversation history (multi-turn) and returns the completion text.
        /// Useful when you need to maintain context across multiple exchanges.
        /// </summary>
        public async Task<string> GenerateExplanationAsync(Dictionary<string, string> fields, PdfMetadata metadata, int score)
        {
            float temperature = 0.7f;
            int maxOutputTokens = 800;
            CancellationToken cancellationToken = default;
            var messages = new List<ChatMessage>
            {
                new SystemChatMessage("You are a fraud detection assistant."),
                new UserChatMessage($"Fields: {string.Join(", ", fields)} | Metadata: {metadata.Producer} | Score: {score}")
            };

            var options = new ChatCompletionOptions
            {
                Temperature = temperature,
                MaxOutputTokenCount = maxOutputTokens
            };
            ChatClient chatClient = _azureOpenAIClient.GetChatClient(_deployment);
            ChatCompletion completion = await chatClient.CompleteChatAsync(
                messages, options, cancellationToken);

            return completion.Content.Count > 0
                ? completion.Content[0].Text
                : string.Empty;
        }

    }
}
