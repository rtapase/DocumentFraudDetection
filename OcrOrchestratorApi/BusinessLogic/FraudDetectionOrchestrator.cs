using System.Timers;

namespace OcrOrchestratorApi.BusinessLogic
{
    public class FraudDetectionOrchestrator
    {
        private readonly IOcrClient _ocr;
        private readonly IMetadataExtractor _metadata;
        private readonly IExplanationClient _explanation;
        private readonly ITamperService _tamperService;

        public FraudDetectionOrchestrator(IOcrClient ocr, IMetadataExtractor metadata, IExplanationClient explanation, ITamperService tamperService)
        {
            _ocr = ocr; _metadata = metadata; _explanation = explanation; _tamperService = tamperService;
        }

        public async Task<FraudAssessment> ProcessDocumentAsync(string filePath)
        {
            using var stream = File.OpenRead(filePath);
            var fields = await _ocr.ExtractFieldsAsync(stream);
            var metadata = _metadata.Extract(filePath);
            var images = _metadata.RenderPdfPagesToImages(filePath);
            var tamperResult = await _tamperService.AnalyzeImagesAsync(images);
            //var tamperResult = new TamperResult
            //                    {
            //                        TamperFlag = true, // Pretend tampering was detected
            //                        Details = new List<Dictionary<string, double>>
            //                        {
            //                            new Dictionary<string, double> { { "ela_score", 12.5 } },
            //                            new Dictionary<string, double> { { "ela_score", 8.3 } }
            //                        }
            //                    }; // Mock result for testing
            var score = CalculateRiskScore(fields, metadata, tamperResult);
            var explanation = await _explanation.GenerateExplanationAsync(fields, metadata, score, tamperResult.TamperFlag);

            return new FraudAssessment
            {
                FileName = Path.GetFileName(filePath),
                ExtractedFields = fields,
                Metadata = metadata,
                RiskScore = score,
                Explanation = explanation
            };
        }

        private int CalculateRiskScore(Dictionary<string, string> fields, PdfMetadata metadata, TamperResult tamperResult)
        {
            int score = 0;
            if (metadata.CreationDate != metadata.ModificationDate) score += 20;
            if (fields.ContainsKey("Salary") && decimal.TryParse(fields["Salary"], out var salary) && salary > 8000) score += 25;
            if (metadata.Producer?.Contains("Photoshop") == true) score += 40;
            if (tamperResult.TamperFlag) score += 40;

            return Math.Min(100, score);
        }
    }

    public class FraudAssessment
    {
        public string FileName { get; set; }
        public Dictionary<string, string> ExtractedFields { get; set; }
        public PdfMetadata Metadata { get; set; }
        public int RiskScore { get; set; }
        public string Explanation { get; set; }
    }

    public class PdfMetadata
    {
        public string Author { get; set; }
        public string Creator { get; set; }
        public string Producer { get; set; }
        public string CreationDate { get; set; }
        public string ModificationDate { get; set; }
    }

}
