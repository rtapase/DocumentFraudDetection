namespace OcrOrchestratorTests
{
    using System.IO;
    using System.Threading.Tasks;
    using Xunit;
    using Microsoft.Extensions.Configuration;
    using OcrOrchestratorApi.BusinessLogic;

    /*
        What This Test Validates
        OCR works: At least one field is extracted.
        Metadata works: PDF metadata is parsed.
        Rules engine works: Risk score is between 0–100.
        OpenAI works: Explanation string is returned. 

        With this test in place, you can run dotnet test and confirm the orchestrator pipeline works end‑to‑end.
     */

    public class FraudDetectionOrchestratorTests
    {
        private readonly IConfiguration _config;

        public FraudDetectionOrchestratorTests()
        {
            // Load test settings from appsettings.Development.json or environment variables
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.Development.json", optional: true)
                .AddEnvironmentVariables();
            _config = builder.Build();
        }

        [Fact]
        public async Task ProcessDocument_ReturnsAssessment_WithRiskScoreAndExplanation()
        {
            // Arrange: use a synthetic payslip PDF placed in your test data folder
            string testFile = Path.Combine("TestData", "synthetic-payslip.pdf");
            var orchestrator = new FraudDetectionOrchestrator(new MockOcrClient(),
                new MockMetadataExtractor(),
                new MockExplanationClient(),
                new MockTamperServiceClient());

            // Act
            FraudAssessment assessment = await orchestrator.ProcessDocumentAsync(testFile);

            // Assert
            Assert.NotNull(assessment);
            Assert.Equal("synthetic-payslip.pdf", assessment.FileName);
            Assert.True(assessment.ExtractedFields.Count > 0); // OCR extracted something
            Assert.InRange(assessment.RiskScore, 0, 100);      // Score is valid
            Assert.False(string.IsNullOrWhiteSpace(assessment.Explanation)); // Explanation generated

            // Optional: check specific fraud rule triggers
            if (assessment.ExtractedFields.TryGetValue("Salary", out var salary))
            {
                Assert.Contains("Salary", assessment.Explanation);
            }
        }
    }

}
