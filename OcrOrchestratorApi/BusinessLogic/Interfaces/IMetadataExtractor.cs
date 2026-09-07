namespace OcrOrchestratorApi.BusinessLogic
{
    public interface IMetadataExtractor
    {
        PdfMetadata Extract(string filePath);
        List<byte[]> RenderPdfPagesToImages(string filePath);
    }
}
