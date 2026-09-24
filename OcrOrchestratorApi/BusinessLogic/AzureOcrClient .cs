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
            // Use the "prebuilt-document" model for document analysis
            Operation<AnalyzeResult> analyzeResult = await _client.AnalyzeDocumentAsync(
                WaitUntil.Completed,
                "prebuilt-layout",
                BinaryData.FromStream(documentStream)
            );

            var result = analyzeResult.Value;
            var fields = new Dictionary<string, string>();
            var table = result.Tables[0];
            string key = String.Empty;
            string value = String.Empty;
            for (int i = 0; i < table.RowCount; i++)
            {
                key = String.Empty;
                value = String.Empty;

                foreach (var cell in table.Cells)
                {
                    if (cell.RowIndex == i)
                    {
                        if (cell.Content.Contains(":"))
                        {
                            key = cell.Content.Replace(":", "").Trim();
                        }
                        else
                        {
                            value = cell.Content.Trim();
                        }

                        if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(value))
                        {
                            fields[key] = value;
                            key = String.Empty;
                            value = String.Empty;
                            break; // Move to the next row after finding a key-value pair
                        }
                    }

                }
                
            }
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
