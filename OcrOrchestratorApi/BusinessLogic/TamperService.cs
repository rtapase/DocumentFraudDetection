namespace OcrOrchestratorApi.BusinessLogic
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
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
            try
            {
                using var ms = new MemoryStream(imgBytes);
                using var bmp = new Bitmap(ms);

                // Very simple heuristic: average pixel intensity variance
                double variance = 0;
                int count = 0;

                for (int x = 0; x < bmp.Width; x += 10)
                {
                    for (int y = 0; y < bmp.Height; y += 10)
                    {
                        var pixel = bmp.GetPixel(x, y);
                        double intensity = (pixel.R + pixel.G + pixel.B) / 3.0;
                        variance += Math.Abs(intensity - 128); // deviation from mid‑tone
                        count++;
                    }
                }

                return count > 0 ? variance / count : 0;
            }
            catch
            {
                // If image parsing fails, return high score to simulate suspicion
                return 50.0;
            }
        }
    }

    public class TamperResult
    {
        public bool TamperFlag { get; set; }
        public List<Dictionary<string, double>> Details { get; set; }
    }

}
