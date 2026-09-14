using System.Timers;

namespace OcrOrchestratorApi.BusinessLogic
{
    public class FraudDetectionOrchestrator
    {
        private readonly IOcrClient _ocr;
        private readonly IMetadataExtractor _metadata;
        private readonly IExplanationClient _explanation;
        private readonly TamperService _tamperService;

        public FraudDetectionOrchestrator(IOcrClient ocr, IMetadataExtractor metadata, IExplanationClient explanation, TamperService tamperService)
        {
            _ocr = ocr; _metadata = metadata; _explanation = explanation; _tamperService = tamperService;
        }

        public async Task<FraudAssessment> ProcessDocumentAsync(string filePath)
        {
            using var stream = File.OpenRead(filePath);
            var fields = await _ocr.ExtractFieldsAsync(stream);
            var metadata = _metadata.Extract(filePath);
            var images = _metadata.RenderPdfPagesToImages(filePath);

            // Assumption: RenderPdfPagesToImages returns one byte[] per rendered page.
            // If it actually returns Streams, System.Drawing.Image objects, or file
            // paths, swap the .Select() below for the matching conversion.
            var imageStreams = images
                .Select((imageBytes, index) => (
                    FileName: $"{Path.GetFileNameWithoutExtension(filePath)}_page{index + 1}.jpg",
                    Content: (Stream)new MemoryStream(imageBytes)))
                .ToList();

            TamperAnalysisResponse tamperResult;
            try
            {
                // TamperService.Analyze is synchronous and CPU-bound (JPEG re-encode +
                // pixel diffing per page), so it's offloaded via Task.Run rather than
                // awaited directly — that keeps it from blocking the thread that's
                // running the rest of this async pipeline.
                tamperResult = await Task.Run(() => _tamperService.Analyze(imageStreams));
            }
            finally
            {
                foreach (var (_, content) in imageStreams)
                    content.Dispose();
            }



            var score = CalculateRiskScore(fields, metadata, tamperResult);
            var explanation = await _explanation.GenerateExplanationAsync(fields, metadata, score, tamperResult.TamperFlag);

            return new FraudAssessment
            {
                FileName = Path.GetFileName(filePath),
                TamperDetected = tamperResult.TamperFlag,
                ExtractedFields = fields,
                RiskScore = score,
                Explanation = explanation
            };
        }

        private int CalculateRiskScore(Dictionary<string, string> fields, PdfMetadata metadata, TamperAnalysisResponse tamperResult)
        {
            int score = 0;
            if (metadata.CreationDate != metadata.ModificationDate) score += 20;
            if (fields.ContainsKey("Salary") && decimal.TryParse(fields["Salary"], out var salary) && salary > 8000) score += 25;
            if (String.IsNullOrEmpty(metadata.Producer) || metadata.Producer?.Contains("Photoshop") == true) score += 40;
            if(String.IsNullOrEmpty(metadata.Author) || String.IsNullOrEmpty(metadata.Creator)) score += 25;
            if (fields.ContainsKey("Date of Issue") && DateTime.TryParse(fields["Date of Issue"], out var issueDate) && issueDate > DateTime.Now) score += 25;
            if (tamperResult.TamperFlag) score += 40;

            return Math.Min(100, score);
        }
    }

    public class FraudAssessment
    {
        public string FileName { get; set; }
        public bool TamperDetected { get; set; }
        public Dictionary<string, string> ExtractedFields { get; set; }
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
