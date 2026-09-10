using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OcrOrchestratorApi.BusinessLogic;

namespace OcrOrchestratorApi.Controllers
{
    [ApiController]
    [Route("api/frauddetection")]
    public class FraudDetectionController : ControllerBase
    {
        private readonly FraudDetectionOrchestrator _orchestrator;

        public FraudDetectionController(FraudDetectionOrchestrator orchestrator)
        {
            _orchestrator = orchestrator;
        }

        [HttpPost("analyze")]
        public async Task<IActionResult> Analyze(IFormFile file)
        {
            if (file == null || file.Length == 0) return BadRequest("No file uploaded");

            var path = Path.GetTempFileName();
            using (var stream = System.IO.File.Create(path))
            {
                await file.CopyToAsync(stream);
            }

            var assessment = await _orchestrator.ProcessDocumentAsync(path);
            return Ok(assessment);
        }

        [HttpGet("analyzedocument")]
        public async Task<IActionResult> AnalyzeDocument()
        {
            var filePath = "D:\\Rahul\\Technical\\Development\\Data\\sample.pdf"; 
            
            var assessment = await _orchestrator.ProcessDocumentAsync(filePath);
            return Ok(assessment);
        }
    }

}
