using OpenAI;
using OpenAI.Chat;


namespace OcrOrchestratorApi.BusinessLogic
{
    public class AzureExplanationClient : IExplanationClient
    {
        private readonly OpenAIClient _client;
        private readonly string _deployment;
        public AzureExplanationClient(OpenAIClient client, string deployment)
        {
            _client = client; _deployment = deployment;
        }

        public async Task<string> GenerateExplanationAsync(Dictionary<string, string> fields, PdfMetadata metadata, int score)
        {
            //var chatOptions = new ChatCompletionsOptions
            //{
            //    Messages =
            //{
            //    new ChatMessage(ChatMessageRole.System,"You are a fraud detection assistant."),
            //    new ChatMessage(ChatMessageRole.User,$"Fields: {string.Join(", ", fields)} | Metadata: {metadata.Producer} | Score: {score}")
            //},
            //    MaxTokens = 200
            //};
            var messages = new List<ChatMessage>
            {
                new SystemChatMessage("You are a fraud detection assistant."),
                new UserChatMessage($"Fields: {string.Join(", ", fields)} | Metadata: {metadata.Producer} | Score: {score}")
            };
            //var resp = await _client.GetChatCompletionsAsync(_deployment, chatOptions);
            var chatClient =  _client.GetChatClient(_deployment);
            var resp = await chatClient.CompleteChatAsync(messages);
            return resp.Value.Content[0].Text;
            //return resp.Value.Choices[0].Message.Content;
        }
    }
}
