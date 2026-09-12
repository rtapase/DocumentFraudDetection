namespace OcrOrchestratorApi.BusinessLogic
{
    public interface ITamperService
    {
        Task<TamperResult> AnalyzeImagesAsync(List<byte[]> images);
    }
}