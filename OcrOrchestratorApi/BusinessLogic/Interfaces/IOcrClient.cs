namespace OcrOrchestratorApi.BusinessLogic
{
    public interface IOcrClient
    {
        Task<Dictionary<string, string>> ExtractFieldsAsync(Stream documentStream);
    }
}
