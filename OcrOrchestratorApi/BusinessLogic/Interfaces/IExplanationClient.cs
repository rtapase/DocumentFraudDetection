namespace OcrOrchestratorApi.BusinessLogic
{
    public interface IExplanationClient
    {
        Task<string> GenerateExplanationAsync(Dictionary<string, string> fields, PdfMetadata metadata, int score);
    }
}
