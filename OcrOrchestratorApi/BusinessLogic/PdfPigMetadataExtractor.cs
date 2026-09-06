using UglyToad.PdfPig;

namespace OcrOrchestratorApi.BusinessLogic
{
    public class PdfPigMetadataExtractor : IMetadataExtractor
    {
        public PdfMetadata Extract(string filePath)
        {
            using var doc = PdfDocument.Open(filePath);
            var info = doc.Information;
            return new PdfMetadata
            {
                Author = info.Author,
                Creator = info.Creator,
                Producer = info.Producer,
                CreationDate = info.CreationDate?.ToString(),
                ModificationDate = info.ModifiedDate?.ToString()
            };
        }
    }
}
