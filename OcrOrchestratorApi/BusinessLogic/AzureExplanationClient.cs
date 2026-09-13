namespace OcrOrchestratorApi.BusinessLogic
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Azure;
    using Azure.AI.OpenAI;
    using OpenAI.Chat;
    public class AzureExplanationClient : IExplanationClient
    {
        private readonly AzureOpenAIClient _azureOpenAIClient;
        private readonly string _deployment;
        public AzureExplanationClient(AzureOpenAIClient azureOpenAIClient, string deployment)
        {
            _azureOpenAIClient = azureOpenAIClient;
            _deployment = deployment;
        }

        public async Task<string> GenerateExplanationAsync(Dictionary<string, string> fields, PdfMetadata metadata, int score, bool tamperDetected)
        {
            float temperature = 0.7f;
            int maxOutputTokens = 800;
            CancellationToken cancellationToken = default;
            string userPrompt = $@"
                                You are a fraud detection analyst. 
                                Given the extracted fields, PDF metadata, and tamper detection results, 
                                provide a clear fraud risk explanation. Consider only the following rules:
                                - Include the tamper detection result in your explanation.
                                - If Salary field is present and Designation is Software Engineer, flag it as suspicious only if it is outside the range of 2000 to 5000.
                                - If the document Created date and Modified date are different, flag it as suspicious.
                                - If the Producer field does not contain 'Microsoft® Word', flag it as suspicious.
                                - If the Author field is not 'HR Dept', flag it as suspicious.
                                - For the 'Date Of Issue' field, consider the 'dd-MM-yyyy' date format.
                                - If the 'Date Of Issue' field is present and is in the future, only then flag it as suspicious. Consider the current date to be {DateTime.Today:dd-MM-yyyy} in your explanation.
                                - Highlight anomalies (e.g., mismatched dates, suspicious producers, unusually high salary).
                                - Connect each anomaly to fraud risk.
                                - Conclude with a recommendation for manual review.

                                Extracted Fields: {string.Join(", ", fields.Select(kvp => $"{kvp.Key}={kvp.Value}"))}
                                Metadata: Author={metadata.Author}, Creator={metadata.Creator}, Producer={metadata.Producer}, Created={metadata.CreationDate}, Modified={metadata.ModificationDate}
                                Risk Score: {score}
                                Tamper Detected: {tamperDetected}";

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
