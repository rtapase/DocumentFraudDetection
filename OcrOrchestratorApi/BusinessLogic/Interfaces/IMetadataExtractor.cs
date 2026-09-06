namespace OcrOrchestratorApi.BusinessLogic
{
    public interface IMetadataExtractor
    {
        PdfMetadata Extract(string filePath);
    }
}
