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
            //var op = await _client.AnalyzeDocumentAsync(WaitUntil.Completed, "prebuilt-document", BinaryData.FromStream(documentStream));
            // Use the "prebuilt-read" model for OCR text extraction
            Operation<AnalyzeResult> analyzeResult = await _client.AnalyzeDocumentAsync(
                WaitUntil.Completed,
                "prebuilt-read",
                BinaryData.FromStream(documentStream)
            );

            var result = analyzeResult.Value;
            var fields = new Dictionary<string, string>();
            foreach (var doc in result.Documents)
                foreach (var field in doc.Fields)
                    fields[field.Key] = field.Value.Content;
            return fields;
        }

        public async Task<StructuredOcrResult> AnalyzeDocumentAsync(Stream documentStream, string fileName)
        {
            // Use the "prebuilt-read" model for OCR text extraction
            Operation<AnalyzeResult> operation = await _client.AnalyzeDocumentAsync(
                WaitUntil.Completed,
                "prebuilt-read",
                BinaryData.FromStream(documentStream)
            );

            AnalyzeResult result = operation.Value;

            var structuredResult = new StructuredOcrResult
            {
                FileName = fileName,
                FullText = result.Content,
                Pages = result.Pages.Select(p => new OcrPage
                {
                    PageNumber = p.PageNumber,
                    Lines = p.Lines.Select(l => l.Content).ToList()
                }).ToList()
            };

            return structuredResult;
        }
    }


}
