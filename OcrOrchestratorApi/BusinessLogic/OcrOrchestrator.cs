namespace OcrOrchestratorApi.BusinessLogic
{
    using Azure;
    using Azure.AI.DocumentIntelligence;

    public class OcrOrchestrator
    {
        private readonly DocumentIntelligenceClient _client;

        public OcrOrchestrator(string endpoint, string apiKey)
        {
            var credential = new AzureKeyCredential(apiKey);
            _client = new DocumentIntelligenceClient(new Uri(endpoint), credential);
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

    public class StructuredOcrResult
    {
        public string FileName { get; set; }
        public string FullText { get; set; }
        public List<OcrPage> Pages { get; set; }
    }

    public class OcrPage
    {
        public int PageNumber { get; set; }
        public List<string> Lines { get; set; }
    }


}
