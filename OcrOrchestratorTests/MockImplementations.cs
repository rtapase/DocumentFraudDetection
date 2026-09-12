using OcrOrchestratorApi.BusinessLogic;
using System;
using System.Collections.Generic;
using System.Text;

namespace OcrOrchestratorTests
{
    public class MockOcrClient : IOcrClient
    {
        public Task<Dictionary<string, string>> ExtractFieldsAsync(Stream documentStream)
        {
            // Return synthetic OCR fields
            return Task.FromResult(new Dictionary<string, string>
        {
            { "EmployeeName", "John Doe" },
            { "Salary", "9000" }
        });
        }
    }

    public class MockMetadataExtractor : IMetadataExtractor
    {
        public PdfMetadata Extract(string filePath)
        {
            return new PdfMetadata
            {
                Author = "TestAuthor",
                Creator = "Word",
                Producer = "Photoshop",
                CreationDate = "2026-01-01",
                ModificationDate = "2026-08-01"
            };
        }

        // Mock implementation of RenderPdfPagesToImages
        public List<byte[]> RenderPdfPagesToImages(string filePath)
        {
            // Instead of rendering real pages, return synthetic byte arrays
            var images = new List<byte[]>();

            // Example: generate 2 fake "pages" as byte arrays
            images.Add(Encoding.UTF8.GetBytes("MockImagePage1"));
            images.Add(Encoding.UTF8.GetBytes("MockImagePage2"));

            return images;
        }
    }

    public class MockExplanationClient : IExplanationClient
    {
        public Task<string> GenerateExplanationAsync(Dictionary<string, string> fields, PdfMetadata metadata, int score, bool tamperDetected)
        {
            return Task.FromResult($"Mock explanation: Salary={fields["Salary"]}, Score={score}, Producer={metadata.Producer}, Tamper Detected={tamperDetected}");
        }
    }

    public class MockTamperServiceClient : ITamperService
    {
        public async Task<TamperResult> AnalyzeImagesAsync(List<byte[]> images)
        {
            // Simulate processing delay
            await Task.Delay(50);

            // Always return a deterministic result for testing
            return new TamperResult
            {
                TamperFlag = true, // Pretend tampering was detected
                Details = new List<Dictionary<string, double>>
                {
                    new Dictionary<string, double> { { "ela_score", 12.5 } },
                    new Dictionary<string, double> { { "ela_score", 8.3 } }
                }
            };

        }
    }


}
