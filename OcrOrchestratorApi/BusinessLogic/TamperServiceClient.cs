namespace OcrOrchestratorApi.BusinessLogic
{
    using System.Net.Http.Json;

    public class TamperServiceClient : ITamperServiceClient
    {
        private readonly HttpClient _httpClient;

        public TamperServiceClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<TamperResult> AnalyzeImagesAsync(List<byte[]> images)
        {
            using var content = new MultipartFormDataContent();
            for (int i = 0; i < images.Count; i++)
            {
                content.Add(new ByteArrayContent(images[i]), "images", $"page{i}.jpg");
            }

            var response = await _httpClient.PostAsync("/analyze", content);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<TamperResult>();
        }
    }

    public class TamperResult
    {
        public bool TamperFlag { get; set; }
        public List<Dictionary<string, double>> Details { get; set; }
    }

}
