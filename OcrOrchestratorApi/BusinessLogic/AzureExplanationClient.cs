using Azure.AI.OpenAI;
using OpenAI.Chat;

namespace OcrOrchestratorApi.BusinessLogic
{
    public class AzureExplanationClient : IExplanationClient
    {
        private readonly AzureOpenAIClient _client;
        private readonly string _deployment;
        public AzureExplanationClient(AzureOpenAIClient client, string deployment)
        {
            _client = client; _deployment = deployment;
        }

        public async Task<string> GenerateExplanationAsync(Dictionary<string, string> fields, PdfMetadata metadata, int score)
        {
            var chatOptions = new ChatCompletionsOptions
            {
                Messages =
            {
                new ChatMessage(ChatMessageRole.System,"You are a fraud detection assistant."),
                new ChatMessage(ChatMessageRole.User,$"Fields: {string.Join(", ", fields)} | Metadata: {metadata.Producer} | Score: {score}")
            },
                MaxTokens = 200
            };
            var resp = await _client.GetChatCompletionsAsync(_deployment, chatOptions);
            return resp.Value.Choices[0].Message.Content;
        }
    }
}
