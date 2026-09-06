using OcrOrchestratorApi.BusinessLogic;
using System;
using System.Collections.Generic;
using System.Text;

namespace OcrOrchestratorTests
{
    public class UnitTest_WithMocks
    {
        [Fact]
        public async Task ProcessDocument_UsesMocks_ReturnsExpectedAssessment()
        {
            var orchestrator = new FraudDetectionOrchestrator(
                new MockOcrClient(),
                new MockMetadataExtractor(),
                new MockExplanationClient()
            );

            var assessment = await orchestrator.ProcessDocumentAsync("fake.pdf");

            Assert.Equal("fake.pdf", assessment.FileName);
            Assert.Equal(85, assessment.RiskScore); // 20 + 25 + 40
            Assert.Contains("Mock explanation", assessment.Explanation);
        }

    }
}
