namespace OcrOrchestratorApi.BusinessLogic
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Threading.Tasks;

    public class TamperService : ITamperService
    {
        /// <summary>
        /// Analyze a list of images (PDF pages rendered as byte arrays) for tampering.
        /// </summary>
        public async Task<TamperResult> AnalyzeImagesAsync(List<byte[]> images)
        {
            // Simulate processing delay
            await Task.Delay(100);

            var details = new List<Dictionary<string, double>>();
            bool tamperFlag = false;

            foreach (var imgBytes in images)
            {
                double elaScore = ComputeElaScore(imgBytes);
                details.Add(new Dictionary<string, double> { { "ela_score", elaScore } });

                // Simple heuristic: flag tampering if score exceeds threshold
                if (elaScore > 10.0)
                    tamperFlag = true;
            }

            return new TamperResult
            {
                TamperFlag = tamperFlag,
                Details = details
            };
        }

        /// <summary>
        /// Compute a synthetic Error Level Analysis (ELA) score.
        /// In production, replace with actual ELA algorithm.
        /// </summary>
        private double ComputeElaScore(byte[] imgBytes)
        {
            using var originalStream = new MemoryStream(imgBytes);
            using var originalBmp = new Bitmap(originalStream);

            // Recompress at JPEG quality 90
            using var recompressedStream = new MemoryStream();
            var encoder = ImageCodecInfo.GetImageEncoders()
                .First(c => c.FormatID == ImageFormat.Jpeg.Guid);
            var encoderParams = new EncoderParameters(1);
            encoderParams.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 90L);
            originalBmp.Save(recompressedStream, encoder, encoderParams);

            recompressedStream.Position = 0;
            using var recompressedBmp = new Bitmap(recompressedStream);

            double diffSum = 0;
            int count = 0;

            for (int x = 0; x < originalBmp.Width; x += 10)
            {
                for (int y = 0; y < originalBmp.Height; y += 10)
                {
                    var o = originalBmp.GetPixel(x, y);
                    var r = recompressedBmp.GetPixel(x, y);

                    double diff = Math.Abs(o.R - r.R) + Math.Abs(o.G - r.G) + Math.Abs(o.B - r.B);
                    diffSum += diff;
                    count++;
                }
            }

            return count > 0 ? diffSum / count : 0;
        }

    }

    public class TamperResult
    {
        public bool TamperFlag { get; set; }
        public List<Dictionary<string, double>> Details { get; set; }
    }

}
