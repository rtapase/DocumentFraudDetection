namespace OcrOrchestratorApi.BusinessLogic
{
    using Azure;
    using Azure.AI.DocumentIntelligence;
    using Azure.AI.OpenAI;
    using Microsoft.Extensions.Configuration;
    using OpenAI;
    using OpenAI.Chat;
    using UglyToad.PdfPig;

    public class FraudDetectionOrchestrator_old
    {
        //        private readonly DocumentIntelligenceClient _docClient;
        //        private readonly OpenAIClient _openAiClient;
        //        private readonly string _openAiDeployment;

        //        public FraudDetectionOrchestrator_old(IConfiguration config)
        //        {
        //            // Azure Document Intelligence
        //            string endpoint = config["AzureDocumentIntelligence:Endpoint"];
        //            string apiKey = config["AzureDocumentIntelligence:ApiKey"];
        //            _docClient = new DocumentIntelligenceClient(new Uri(endpoint), new AzureKeyCredential(apiKey));

        //            // Azure OpenAI
        //            string openAiEndpoint = config["AzureOpenAI:Endpoint"];
        //            string openAiKey = config["AzureOpenAI:ApiKey"];
        //            _openAiDeployment = config["AzureOpenAI:DeploymentName"]; // e.g. "fraud-detector-gpt4"
        //            //_openAiClient = new OpenAIClient(new Uri(openAiEndpoint), );
        //            _openAiClient = new OpenAIClient(new AzureKeyCredential(openAiKey));
        //        }

        //        public async Task<FraudAssessment> ProcessDocumentAsync(string filePath)
        //        {
        //            using var stream = File.OpenRead(filePath);

        //            // 1. OCR structured extraction
        //            var operation = await _docClient.AnalyzeDocumentAsync(
        //                WaitUntil.Completed,
        //                "prebuilt-document",
        //                BinaryData.FromStream(stream)
        //            );
        //            var ocrResult = operation.Value;

        //            var fields = new Dictionary<string, string>();
        //            foreach (var doc in ocrResult.Documents)
        //            {
        //                foreach (var field in doc.Fields)
        //                {
        //                    fields[field.Key] = field.Value.Content;
        //                }
        //            }

        //            // 2. Metadata analysis
        //            var metadata = ExtractPdfMetadata(filePath);

        //            // 3. Fraud rules engine
        //            var score = CalculateRiskScore(fields, metadata);

        //            // 4. Explanation via Azure OpenAI
        //            var explanation = await GenerateExplanation(fields, metadata, score);

        //            return new FraudAssessment
        //            {
        //                FileName = Path.GetFileName(filePath),
        //                ExtractedFields = fields,
        //                Metadata = metadata,
        //                RiskScore = score,
        //                Explanation = explanation
        //            };
        //        }

        //        private PdfMetadata ExtractPdfMetadata(string path)
        //        {
        //            using var doc = PdfDocument.Open(path);
        //            var info = doc.Information;
        //            return new PdfMetadata
        //            {
        //                Author = info.Author,
        //                Creator = info.Creator,
        //                Producer = info.Producer,
        //                CreationDate = info.CreationDate?.ToString(),
        //                ModificationDate = info.ModifiedDate?.ToString()
        //            };
        //        }

        //        private int CalculateRiskScore(Dictionary<string, string> fields, PdfMetadata metadata)
        //        {
        //            int score = 0;

        //            if (!string.IsNullOrEmpty(metadata.ModificationDate) &&
        //                metadata.CreationDate != metadata.ModificationDate)
        //                score += 20;

        //            if (fields.ContainsKey("Salary") &&
        //                decimal.TryParse(fields["Salary"], out var salary) &&
        //                salary > 8000)
        //                score += 25;

        //            if (metadata.Producer?.Contains("Photoshop") == true)
        //                score += 40;

        //            return Math.Min(100, score);
        //        }

        //        private async Task<string> GenerateExplanation(Dictionary<string, string> fields, PdfMetadata metadata, int score)
        //        {
        //            string prompt = $@"
        //Document Fraud Assessment
        //Extracted Fields: {string.Join(", ", fields.Select(kvp => $"{kvp.Key}={kvp.Value}"))}
        //Metadata: Author={metadata.Author}, Creator={metadata.Creator}, Producer={metadata.Producer}, Created={metadata.CreationDate}, Modified={metadata.ModificationDate}
        //Risk Score: {score}

        //Explain why this document may be suspicious and provide a recommendation for manual review.";

        //            var chatOptions = new ChatCompletionsOptions
        //            {
        //                Messages =
        //            {
        //                new ChatMessage(ChatRole.System, "You are a fraud detection assistant."),
        //                new ChatMessage(ChatRole.User, prompt)
        //            },
        //                MaxTokens = 300
        //            };

        //            Response<ChatCompletions> response =
        //                await _openAiClient.GetChatCompletionsAsync(_openAiDeployment, chatOptions);

        //            return response.Value.Choices[0].Message.Content;
        //        }
        //    }

        //    public class FraudAssessment
        //    {
        //        public string FileName { get; set; }
        //        public Dictionary<string, string> ExtractedFields { get; set; }
        //        public PdfMetadata Metadata { get; set; }
        //        public int RiskScore { get; set; }
        //        public string Explanation { get; set; }
        //    }

        //    public class PdfMetadata
        //    {
        //        public string Author { get; set; }
        //        public string Creator { get; set; }
        //        public string Producer { get; set; }
        //        public string CreationDate { get; set; }
        //        public string ModificationDate { get; set; }
        //    }


    }
}
