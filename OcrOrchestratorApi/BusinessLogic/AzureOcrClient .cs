using Azure;
using Azure.AI.DocumentIntelligence;

namespace OcrOrchestratorApi.BusinessLogic
{
    public class AzureOcrClient : IOcrClient
    {
        private readonly DocumentIntelligenceClient _client;
        public AzureOcrClient(DocumentIntelligenceClient client) => _client = client;

        public async Task<Dictionary<string, string>> ExtractFieldsAsync(Stream documentStream)
        {
            var op = await _client.AnalyzeDocumentAsync(WaitUntil.Completed, "prebuilt-document", BinaryData.FromStream(documentStream));
            var result = op.Value;
            var fields = new Dictionary<string, string>();
            foreach (var doc in result.Documents)
                foreach (var field in doc.Fields)
                    fields[field.Key] = field.Value.Content;
            return fields;
        }
    }
}
