namespace OcrOrchestratorApi.BusinessLogic
{
    public interface ITamperServiceClient
    {
        Task<TamperResult> AnalyzeImagesAsync(List<byte[]> images);
    }
}