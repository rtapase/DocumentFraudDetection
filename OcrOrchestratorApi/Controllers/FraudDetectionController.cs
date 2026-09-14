using Microsoft.AspNetCore.Http;
using System.Configuration;
using Microsoft.AspNetCore.Mvc;
using OcrOrchestratorApi.BusinessLogic;

namespace OcrOrchestratorApi.Controllers
{
    [ApiController]
    [Route("api/frauddetection")]
    public class FraudDetectionController : ControllerBase
    {
        private readonly FraudDetectionOrchestrator _orchestrator;
        private readonly IConfiguration _configuration;

        public FraudDetectionController(FraudDetectionOrchestrator orchestrator, IConfiguration configuration)
        {
            _orchestrator = orchestrator;
            _configuration = configuration;
        }

        [HttpPost("analyze")]
        public async Task<IActionResult> Analyze(IFormFile file)
        {
            if (file == null || file.Length == 0) return BadRequest("No file uploaded");
            var fileName = Path.GetFileName(file.FileName);
            var path = Path.GetTempFileName();
            using (var stream = System.IO.File.Create(path))
            {
                await file.CopyToAsync(stream);
            }

            var assessment = await _orchestrator.ProcessDocumentAsync(path);
            assessment.FileName = fileName; // Set the original file name
            return Ok(assessment);
        }

        [HttpGet("analyzedocument")]
        public async Task<IActionResult> AnalyzeDocument()
        {
            var filePath = "D:\\Rahul\\Technical\\Development\\FraudDetectionTesting\\SamplePayslip_TAMPERED.pdf"; 
            
            var assessment = await _orchestrator.ProcessDocumentAsync(filePath);
            return Ok(assessment);
        }

        [HttpGet("VerifyOcr")]
        public async Task<IActionResult> VerifyOcr()
        {
            var filePath = "D:\\Rahul\\Technical\\Development\\Data\\sample.pdf";
            
            string endpoint = _configuration["AzureDocumentIntelligence:Endpoint"];
            string apiKey = _configuration["AzureDocumentIntelligence:ApiKey"];
            using var stream = System.IO.File.OpenRead(filePath);
            {
                var ocrOrchestrator = new OcrOrchestrator(endpoint, apiKey);
                var result = await ocrOrchestrator.AnalyzeDocumentAsync(stream, Path.GetFileName(filePath));
            }

            

            var assessment = await _orchestrator.ProcessDocumentAsync(filePath);
            return Ok(assessment);
        }

        [HttpGet("GenerateExplanation")]
        public async Task<IActionResult> GenerateExplanation()
        {
            string endpoint = _configuration["AzureOpenAI:Endpoint"];
            string apiKey = _configuration["AzureOpenAI:ApiKey"];
            string deployment = _configuration["AzureOpenAI:DeploymentName"];

            var openaiOrchestrator = new OpenAiOrchestrator(endpoint, apiKey, deployment);
            var explanation = await openaiOrchestrator.GenerateExplanationAsync(new Dictionary<string, string>(), new PdfMetadata(), 0);

            return Ok(explanation);
        }
    }

}
