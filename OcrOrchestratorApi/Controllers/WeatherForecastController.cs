using Microsoft.AspNetCore.Mvc;

namespace OcrOrchestratorApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries =
        [
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        ];

        [HttpGet(Name = "GetWeatherForecast")]
        public IEnumerable<WeatherForecast> Get()
        {
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
        }

        private void UseFraudDetectionOrchestrator()
        {
            // Example usage of FraudDetectionOrchestrator
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();
            //var orchestrator = new FraudDetectionOrchestrator(config);
            //var result = orchestrator.ProcessDocumentAsync("path/to/document.pdf").Result;
            // Handle the result as needed
            /*
            var orchestrator = new FraudDetectionOrchestrator(config);
            var assessment = await orchestrator.ProcessDocumentAsync("sample-payslip.pdf");

            Console.WriteLine($"File: {assessment.FileName}");
            Console.WriteLine($"Risk Score: {assessment.RiskScore}");
            Console.WriteLine("Explanation:");
            Console.WriteLine(assessment.Explanation);
            */

        }
    }
}
