using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using System.Drawing;
using System.Drawing.Imaging;

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

        public List<byte[]> RenderPdfPagesToImages(string filePath)
        {
            var images = new List<byte[]>();

            using var document = PdfDocument.Open(filePath);

            foreach (var page in document.GetPages())
            {
                // Create a bitmap canvas (adjust resolution as needed)
                int width = (int)page.Width;
                int height = (int)page.Height;
                using var bmp = new Bitmap(width, height);
                using var g = Graphics.FromImage(bmp);

                g.Clear(Color.White);

                // Draw text content
                foreach (var letter in page.Letters)
                {
                    using var font = new Font("Arial", (float)letter.PointSize);
                    g.DrawString(letter.Value, font, Brushes.Black,
                        (float)letter.Location.X, (float)(height - letter.Location.Y));
                }

                // Convert bitmap to byte[] (JPEG for tamper service)
                using var ms = new MemoryStream();
                bmp.Save(ms, ImageFormat.Jpeg);
                images.Add(ms.ToArray());
            }

            return images;
        }

    }

    /* usage example:
     var images = PdfImageRenderer.RenderPdfPagesToImages("sample-payslip.pdf");
    var tamperResult = await _tamperClient.AnalyzeImagesAsync(images);

     */


}
