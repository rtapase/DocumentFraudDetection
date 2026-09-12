namespace OcrOrchestratorApi.BusinessLogic
{
    using Azure;
    using Azure.AI.OpenAI;
    using OpenAI.Chat;
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Timers;
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
            string userPrompt = $@"
                                You are a fraud detection analyst. 
                                Given the extracted fields, PDF metadata, and tamper detection results, 
                                provide a clear fraud risk explanation.

                                - Highlight anomalies (e.g., mismatched dates, suspicious producers, unusually high salary).
                                - Connect each anomaly to fraud risk.
                                - Conclude with a recommendation for manual review.

                                Extracted Fields: {string.Join(", ", fields.Select(kvp => $"{kvp.Key}={kvp.Value}"))}
                                Metadata: Author={metadata.Author}, Creator={metadata.Creator}, Producer={metadata.Producer}, Created={metadata.CreationDate}, Modified={metadata.ModificationDate}
                                Risk Score: {score}";

            var messages = new List<ChatMessage>
            {
                new SystemChatMessage("You are a fraud detection analyst."),
                new UserChatMessage(userPrompt)
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
