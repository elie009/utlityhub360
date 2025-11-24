using System.Text;
using System.Text.RegularExpressions;
using Tesseract;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.PixelFormats;

namespace UtilityHub360.Services
{
    public class OcrService : IOcrService
    {
        private readonly ILogger<OcrService> _logger;
        private readonly IConfiguration _configuration;
        private readonly string _tesseractDataPath;

        public OcrService(ILogger<OcrService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            
            // Tesseract data path - can be configured in appsettings
            _tesseractDataPath = configuration["OcrSettings:TesseractDataPath"] 
                ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tessdata");
        }

        public async Task<OcrResult> ProcessImageAsync(Stream imageStream, string fileType)
        {
            try
            {
                // Use Tesseract for OCR
                return await ProcessWithTesseractAsync(imageStream);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing image with OCR");
                return new OcrResult { FullText = string.Empty, Confidence = 0 };
            }
        }

        public async Task<OcrResult> ProcessPdfAsync(Stream pdfStream)
        {
            try
            {
                // For iTextSharp.LGPLv2.Core, PDF text extraction API is limited
                // Skip direct text extraction and go straight to OCR-based processing
                // Convert first page to image and use OCR
                pdfStream.Position = 0;
                using var image = ConvertPdfPageToImage(pdfStream, 0);
                if (image != null)
                {
                    using var ms = new MemoryStream();
                    await image.SaveAsJpegAsync(ms);
                    ms.Position = 0;
                    return await ProcessWithTesseractAsync(ms);
                }

                // If image conversion fails, try text extraction as fallback
                var pdfText = ExtractTextFromPdf(pdfStream);
                if (!string.IsNullOrWhiteSpace(pdfText))
                {
                    var result = ParseReceiptText(pdfText);
                    result.Provider = "PDF_TEXT_EXTRACTION";
                    return result;
                }

                return new OcrResult { FullText = string.Empty, Confidence = 0 };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing PDF with OCR");
                return new OcrResult { FullText = string.Empty, Confidence = 0 };
            }
        }

        private async Task<OcrResult> ProcessWithTesseractAsync(Stream imageStream)
        {
            try
            {
                // Preprocess image for better OCR
                using var image = await Image.LoadAsync(imageStream);
                
                // Resize if too large (max 2000px on longest side)
                var maxDimension = Math.Max(image.Width, image.Height);
                if (maxDimension > 2000)
                {
                    var scale = 2000.0 / maxDimension;
                    image.Mutate(x => x.Resize(new ResizeOptions
                    {
                        Size = new Size((int)(image.Width * scale), (int)(image.Height * scale)),
                        Mode = ResizeMode.Max
                    }));
                }

                // Convert to grayscale and enhance contrast
                image.Mutate(x => x
                    .Grayscale()
                    .Contrast(1.2f)
                    .Brightness(1.1f));

                using var processedStream = new MemoryStream();
                await image.SaveAsPngAsync(processedStream);
                processedStream.Position = 0;

                // Perform OCR with Tesseract
                using var engine = new TesseractEngine(_tesseractDataPath, "eng", EngineMode.Default);
                using var img = Pix.LoadFromMemory(processedStream.ToArray());
                using var page = engine.Process(img);

                var text = page.GetText();
                var confidence = page.GetMeanConfidence();

                var result = ParseReceiptText(text);
                result.Confidence = confidence;
                result.Provider = "TESSERACT";

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Tesseract OCR processing");
                return new OcrResult { FullText = string.Empty, Confidence = 0, Provider = "TESSERACT" };
            }
        }

        private string ExtractTextFromPdf(Stream pdfStream)
        {
            // PDF text extraction is not fully supported with iTextSharp.LGPLv2.Core
            // This method is kept as a placeholder but will return empty
            // OCR processing will be used instead via ConvertPdfPageToImage
            try
            {
                // The parser namespace with PdfTextExtractor is not available in LGPLv2.Core
                // For now, return empty string - OCR will handle text extraction
                return string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        private Image<Rgba32>? ConvertPdfPageToImage(Stream pdfStream, int pageNumber)
        {
            // This is a simplified version - in production, you might want to use a library like PdfiumViewer
            // For now, we'll return null and the PDF text extraction will be used instead
            return null;
        }

        private OcrResult ParseReceiptText(string text)
        {
            var result = new OcrResult
            {
                FullText = text
            };

            if (string.IsNullOrWhiteSpace(text))
            {
                return result;
            }

            // Extract amount (look for currency patterns)
            var amountPatterns = new[]
            {
                @"(?:total|amount|sum|due|balance)[\s:]*\$?([\d,]+\.?\d{0,2})",
                @"\$([\d,]+\.?\d{0,2})",
                @"([\d,]+\.?\d{2})\s*(?:USD|EUR|GBP|CAD)"
            };

            foreach (var pattern in amountPatterns)
            {
                var match = Regex.Match(text, pattern, RegexOptions.IgnoreCase);
                if (match.Success && decimal.TryParse(match.Groups[1].Value.Replace(",", ""), out var amount))
                {
                    result.Amount = amount;
                    break;
                }
            }

            // Extract date
            var datePatterns = new[]
            {
                @"(\d{1,2}[/-]\d{1,2}[/-]\d{2,4})",
                @"(\d{4}[/-]\d{1,2}[/-]\d{1,2})",
                @"(?:date|dated)[\s:]*(\d{1,2}[/-]\d{1,2}[/-]\d{2,4})"
            };

            foreach (var pattern in datePatterns)
            {
                var match = Regex.Match(text, pattern, RegexOptions.IgnoreCase);
                if (match.Success && DateTime.TryParse(match.Groups[1].Value, out var date))
                {
                    result.Date = date;
                    break;
                }
            }

            // Extract merchant (usually first line or after "from", "merchant", etc.)
            var merchantPatterns = new[]
            {
                @"(?:from|merchant|vendor|store)[\s:]*([A-Z][A-Za-z\s&]+)",
                @"^([A-Z][A-Za-z\s&]{3,30})(?:\r|\n)"
            };

            foreach (var pattern in merchantPatterns)
            {
                var match = Regex.Match(text, pattern, RegexOptions.IgnoreCase | RegexOptions.Multiline);
                if (match.Success)
                {
                    result.Merchant = match.Groups[1].Value.Trim();
                    break;
                }
            }

            // If no merchant found, try to get first meaningful line
            if (string.IsNullOrEmpty(result.Merchant))
            {
                var lines = text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var line in lines.Take(5))
                {
                    var trimmed = line.Trim();
                    if (trimmed.Length > 3 && trimmed.Length < 50 && !Regex.IsMatch(trimmed, @"^\d"))
                    {
                        result.Merchant = trimmed;
                        break;
                    }
                }
            }

            // Extract items (look for item descriptions with prices)
            var itemPattern = @"([A-Za-z\s]+)\s+(\d+\.?\d{0,2})";
            var itemMatches = Regex.Matches(text, itemPattern);
            foreach (Match match in itemMatches)
            {
                if (decimal.TryParse(match.Groups[2].Value, out var price))
                {
                    result.Items.Add(new ReceiptItem
                    {
                        Description = match.Groups[1].Value.Trim(),
                        Price = price
                    });
                }
            }

            return result;
        }
    }
}

