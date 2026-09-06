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
    }

    public class MockExplanationClient : IExplanationClient
    {
        public Task<string> GenerateExplanationAsync(Dictionary<string, string> fields, PdfMetadata metadata, int score)
        {
            return Task.FromResult($"Mock explanation: Salary={fields["Salary"]}, Score={score}, Producer={metadata.Producer}");
        }
    }

}
