// TamperService.cs
//
// C# port of tamper_service/app.py (Flask + OpenCV + custom ELA module).
//
// SCORING METHOD (updated): ComputeEla() now returns the MAX mean pixel diff
// across fixed-size tiles, not the whole-image mean. The whole-image mean was
// tested against a real payslip render (1654x2339px) and found structurally
// incapable of detecting single-field tampering: a fully corrupted salary
// field only covers ~0.2% of the page, but reaching the old threshold of 10
// required ~3.9% of the entire page to be saturated. Tile-based max scoring
// reacts to a small hot region instead of diluting it across the whole image.
//
// HONEST LIMITATION, found via testing, worth keeping in mind: on crisp,
// vector-rendered/scanned TEXT documents (like a payslip), ordinary untampered
// text edges already produce local JPEG quantization noise of a similar
// magnitude to a doctored text field — a max-tile score alone can't reliably
// tell "edited number" from "just some other bold label" on this kind of
// content. What DOES produce a strong, well-separated signal is genuinely
// high-frequency content in the tampered region (e.g. a pasted photo, stamp,
// signature scan, or anything with real texture/noise) — that's inherently
// hard for JPEG to compress in a single pass, regardless of double-compression
// tricks. Treat this scorer as a real signal for photographic/textured
// tampering, and as a weak-to-absent signal for pure text edits — pair it with
// metadata checks (already partly present elsewhere in this pipeline) for the
// text-editing case specifically.
//
// Differences from ela.py, all intentional:
//   - Default JPEG quality is 95 to match ela.py.
//   - Scoring is tile-max instead of whole-image mean (see above). The default
//     threshold changed from 10.0 (calibrated for whole-image mean) to 3.5
//     (calibrated empirically against one sample document's natural noise
//     ceiling of ~1.0-1.5 vs. a genuinely tampered region's ~7.1) — RECALIBRATE
//     against your own corpus of clean documents before relying on this in
//     production; this default is a starting point, not a universal constant.
//   - No temp file is written/deleted (ImageSharp encodes straight to a
//     MemoryStream; ela.py needed a real file because PIL forces re-encoding
//     through disk).
//   - The brightened visual mask (ImageEnhance.Brightness + enhance_factor) is
//     not ported, since app.py's /analyze route only used the numeric score.
//
// NuGet dependency: SixLabors.ImageSharp
//   dotnet add package SixLabors.ImageSharp

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.PixelFormats;

namespace OcrOrchestratorApi.BusinessLogic
{
    public class ImageAnalysisResult
    {
        public double ElaScore { get; set; } // now: max tile-mean pixel diff, not whole-image mean
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
        private readonly int _tileSize;
        private const long MaxImageBytes = 20 * 1024 * 1024; // 20 MB per image guard

        public TamperService(int jpegQuality = 95, double tamperThreshold = 3.5, int tileSize = 32)
        {
            if (jpegQuality < 1 || jpegQuality > 100)
                throw new ArgumentOutOfRangeException(nameof(jpegQuality), "Must be between 1 and 100.");
            if (tileSize < 1)
                throw new ArgumentOutOfRangeException(nameof(tileSize), "Must be at least 1 pixel.");

            _jpegQuality = jpegQuality;
            _tamperThreshold = tamperThreshold;
            _tileSize = tileSize;
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

            int tilesX = (original.Width + _tileSize - 1) / _tileSize;
            int tilesY = (original.Height + _tileSize - 1) / _tileSize;
            var tileSums = new double[tilesX, tilesY];
            var tileCounts = new long[tilesX, tilesY];

            original.ProcessPixelRows(recompressed, (origAccessor, recompAccessor) =>
            {
                for (int y = 0; y < origAccessor.Height; y++)
                {
                    var origRow = origAccessor.GetRowSpan(y);
                    var recompRow = recompAccessor.GetRowSpan(y);
                    int tileY = y / _tileSize;

                    for (int x = 0; x < origRow.Length; x++)
                    {
                        var o = origRow[x];
                        var r = recompRow[x];

                        int diffR = Math.Abs(o.R - r.R);
                        int diffG = Math.Abs(o.G - r.G);
                        int diffB = Math.Abs(o.B - r.B);
                        double pixelDiff = (diffR + diffG + diffB) / 3.0;

                        int tileX = x / _tileSize;
                        tileSums[tileX, tileY] += pixelDiff;
                        tileCounts[tileX, tileY] += 1;
                    }
                }
            });

            // Score = the single worst tile's mean diff, not the whole image's
            // mean — this is what lets a small localized splice register instead
            // of being diluted across thousands of untouched background pixels.
            double maxTileMean = 0;
            for (int tx = 0; tx < tilesX; tx++)
            {
                for (int ty = 0; ty < tilesY; ty++)
                {
                    if (tileCounts[tx, ty] == 0) continue;
                    double tileMean = tileSums[tx, ty] / tileCounts[tx, ty];
                    if (tileMean > maxTileMean)
                        maxTileMean = tileMean;
                }
            }

            return maxTileMean;
        }
    }
}
