using FraudAssessmentWebUI.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Net.Http.Headers;

namespace FraudAssessmentWebUI.Controllers
{
    public class HomeController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public HomeController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                ModelState.AddModelError("", "Please select a file to upload.");
                return View("Index");
            }

            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri("http://localhost:57703/api/FraudDetection/Analyze"); // adjust to your API base URL

            using var content = new MultipartFormDataContent();
            using var stream = file.OpenReadStream();
            var fileContent = new StreamContent(stream);
            fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/pdf");
            content.Add(fileContent, "file", file.FileName);

            var response = await client.PostAsync("analyze", content);
            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError("", "Error analyzing document.");
                return View("Index");
            }

            var assessment = await response.Content.ReadFromJsonAsync<FraudAssessmentViewModel>();
            return View("Result", assessment);
        }
    }

}
