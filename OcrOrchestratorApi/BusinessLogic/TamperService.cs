// TamperService.cs
//
// C# port of tamper_service/app.py (Flask + OpenCV + custom ELA module).
//
// ComputeEla() below is a direct port of ela.py's compute_ela_score(): recompress
// the image as JPEG at a fixed quality, take the pixel-wise absolute difference
// against the original, and average it. np.mean(diff_array) over an (H, W, 3)
// array is mathematically identical to averaging the R/G/B diffs per pixel and
// then averaging across all pixels, which is what this does.
//
// Differences from ela.py, both intentional:
//   - Default JPEG quality is 95 to match ela.py (not the 90 guessed earlier).
//   - No temp file is written/deleted. ela.py round-trips through
//     'temp_recompressed.jpg' because PIL needs a real file to force re-encoding;
//     ImageSharp can encode straight to a MemoryStream, so the temp file (and its
//     cleanup, and the risk of collisions across concurrent requests) is skipped.
//   - The brightened visual mask (ImageEnhance.Brightness + enhance_factor) is not
//     ported, since app.py's /analyze route only ever used the numeric score, not
//     the image. Say the word if you want a GenerateVisualMask() method added back.
//
// NuGet dependency: SixLabors.ImageSharp
//   dotnet add package SixLabors.ImageSharp

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.PixelFormats;

namespace OcrOrchestratorApi.BusinessLogic
{
    public class ImageAnalysisResult
    {
        public double ElaScore { get; set; }
        public string? Error { get; set; } // per-image error instead of failing the whole batch
    }

    public class TamperAnalysisResponse
    {
        public bool TamperFlag { get; set; }
        public List<ImageAnalysisResult> Details { get; set; } = new();
    }

    public class TamperService
    {
        private readonly int _jpegQuality;
        private readonly double _tamperThreshold;
        private const long MaxImageBytes = 20 * 1024 * 1024; // 20 MB per image guard

        public TamperService(int jpegQuality = 95, double tamperThreshold = 10.0)
        {
            if (jpegQuality < 1 || jpegQuality > 100)
                throw new ArgumentOutOfRangeException(nameof(jpegQuality), "Must be between 1 and 100.");

            _jpegQuality = jpegQuality;
            _tamperThreshold = tamperThreshold;
        }

        /// <summary>
        /// Mirrors the Flask /analyze route: takes a batch of uploaded image streams,
        /// scores each one, and flags the batch as tampered if any score exceeds the threshold.
        /// Unlike the original, a bad/corrupt image is recorded as a per-item error
        /// instead of throwing and killing the whole request.
        /// </summary>
        public TamperAnalysisResponse Analyze(IEnumerable<(string FileName, Stream Content)> images)
        {
            var details = new List<ImageAnalysisResult>();

            if (images == null || !images.Any())
            {
                // The Flask version silently returns tamper_flag: false for a missing/empty
                // 'images' field — surfacing it explicitly here instead.
                throw new ArgumentException("No images were provided under the 'images' field.");
            }

            foreach (var (fileName, content) in images)
            {
                try
                {
                    if (content.Length == 0)
                        throw new InvalidDataException($"'{fileName}' is empty.");

                    if (content.Length > MaxImageBytes)
                        throw new InvalidDataException($"'{fileName}' exceeds the {MaxImageBytes / (1024 * 1024)} MB limit.");

                    double score = ComputeEla(content);
                    details.Add(new ImageAnalysisResult { ElaScore = score });
                }
                catch (Exception ex)
                {
                    details.Add(new ImageAnalysisResult { ElaScore = 0, Error = ex.Message });
                }
            }

            bool tamperFlag = details.Any(d => d.Error == null && d.ElaScore > _tamperThreshold);

            return new TamperAnalysisResponse
            {
                TamperFlag = tamperFlag,
                Details = details
            };
        }

        private double ComputeEla(Stream imageStream)
        {
            using var original = Image.Load<Rgb24>(imageStream);

            // Re-encode at a fixed JPEG quality and reload — this recompression
            // step is what exposes the "error level" differences (mirrors
            // ela.py's original.save(..., 'JPEG', quality=quality) + reopen).
            using var recompressedStream = new MemoryStream();
            original.Save(recompressedStream, new JpegEncoder { Quality = _jpegQuality });
            recompressedStream.Position = 0;
            using var recompressed = Image.Load<Rgb24>(recompressedStream);

            if (original.Width != recompressed.Width || original.Height != recompressed.Height)
                throw new InvalidOperationException("Recompressed image dimensions do not match the original.");

            double totalDiff = 0;
            long pixelCount = (long)original.Width * original.Height;

            original.ProcessPixelRows(recompressed, (origAccessor, recompAccessor) =>
            {
                for (int y = 0; y < origAccessor.Height; y++)
                {
                    var origRow = origAccessor.GetRowSpan(y);
                    var recompRow = recompAccessor.GetRowSpan(y);

                    for (int x = 0; x < origRow.Length; x++)
                    {
                        var o = origRow[x];
                        var r = recompRow[x];

                        int diffR = Math.Abs(o.R - r.R);
                        int diffG = Math.Abs(o.G - r.G);
                        int diffB = Math.Abs(o.B - r.B);

                        totalDiff += (diffR + diffG + diffB) / 3.0;
                    }
                }
            });

            // Average per-pixel error, roughly comparable in scale to the original's threshold of 10.
            return totalDiff / pixelCount;
        }
    }
}
