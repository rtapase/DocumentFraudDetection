namespace OcrOrchestratorApi.BusinessLogic
{
    public class FraudDetectionOrchestrator
    {
        private readonly IOcrClient _ocr;
        private readonly IMetadataExtractor _metadata;
        private readonly IExplanationClient _explanation;

        public FraudDetectionOrchestrator(IOcrClient ocr, IMetadataExtractor metadata, IExplanationClient explanation)
        {
            _ocr = ocr; _metadata = metadata; _explanation = explanation;
        }

        public async Task<FraudAssessment> ProcessDocumentAsync(string filePath)
        {
            using var stream = File.OpenRead(filePath);
            var fields = await _ocr.ExtractFieldsAsync(stream);
            var metadata = _metadata.Extract(filePath);
            var score = CalculateRiskScore(fields, metadata);
            var explanation = await _explanation.GenerateExplanationAsync(fields, metadata, score);

            return new FraudAssessment
            {
                FileName = Path.GetFileName(filePath),
                ExtractedFields = fields,
                Metadata = metadata,
                RiskScore = score,
                Explanation = explanation
            };
        }

        private int CalculateRiskScore(Dictionary<string, string> fields, PdfMetadata metadata)
        {
            int score = 0;
            if (metadata.CreationDate != metadata.ModificationDate) score += 20;
            if (fields.ContainsKey("Salary") && decimal.TryParse(fields["Salary"], out var salary) && salary > 8000) score += 25;
            if (metadata.Producer?.Contains("Photoshop") == true) score += 40;
            return Math.Min(100, score);
        }
    }

}
