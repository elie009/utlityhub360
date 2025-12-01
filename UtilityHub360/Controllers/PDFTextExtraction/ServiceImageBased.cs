using System.Text;
using System.Linq;
using UglyToad.PdfPig;
using PdfPigPage = UglyToad.PdfPig.Content.Page;
using SixLabors.ImageSharp;
using UtilityHub360.Services;

namespace UtilityHub360.Controllers.PDFTextExtraction
{
    public class ServiceImageBased : PDFExtractionBase
    {
        private readonly ILogger<ServiceImageBased> _logger;
        private readonly IOcrService _ocrService;

        public ServiceImageBased(ILogger<ServiceImageBased> logger, IOcrService ocrService)
        {
            _logger = logger;
            _ocrService = ocrService;
        }

        public override async Task<string> ExtractTextFromPDFAsync(Stream pdf)
        {
            try
            {
                string extractedText = "";
                pdf.Position = 0;
                _logger.LogInformation("Attempting to convert scanned PDF to text-based PDF...");

                // Convert scanned PDF to text-based PDF
                var textBasedPdfStream = await _ocrService.ConvertPdfToTextBasedPdfAsync(pdf);

                if (textBasedPdfStream != null && textBasedPdfStream.Length > 0)
                {
                    // Now try PdfPig on the converted PDF
                    textBasedPdfStream.Position = 0;
                    var textBuilder = new StringBuilder();

                    using (var document = PdfDocument.Open(textBasedPdfStream))
                    {
                        var pageCount = document.NumberOfPages;
                        _logger.LogInformation($"Converted PDF has {pageCount} pages");

                        foreach (var page in document.GetPages())
                        {
                            try
                            {
                                var words = page.GetWords();
                                if (words != null && words.Any())
                                {
                                    var pageText = string.Join(" ", words.Select(w => w.Text));
                                    if (!string.IsNullOrWhiteSpace(pageText))
                                    {
                                        textBuilder.AppendLine(pageText);
                                    }
                                }
                            }
                            catch (Exception pageEx)
                            {
                                _logger.LogWarning($"Error extracting text from converted PDF page {page.Number}: {pageEx.Message}");
                            }
                        }
                    }

                    extractedText = textBuilder.ToString();
                    if (!string.IsNullOrWhiteSpace(extractedText))
                    {
                        _logger.LogInformation($"Successfully extracted {extractedText.Length} characters from converted PDF");
                        return extractedText;
                    }
                }

                // If conversion didn't work, try direct OCR extraction as fallback
                if (string.IsNullOrWhiteSpace(extractedText))
                {
                    pdf.Position = 0;
                    _logger.LogInformation("Falling back to direct OCR extraction...");
                    var ocrResult = await _ocrService.ProcessPdfAsync(pdf);

                    if (ocrResult != null && !string.IsNullOrWhiteSpace(ocrResult.FullText))
                    {
                        extractedText = ocrResult.FullText;
                        _logger.LogInformation($"Successfully extracted {extractedText.Length} characters from PDF using {ocrResult.Provider ?? "OCR"}");
                        return extractedText;
                    }
                    else
                    {
                        _logger.LogWarning($"OCR returned empty text. Confidence: {ocrResult?.Confidence ?? 0}");
                    }
                }
            }
            catch (Exception ocrEx)
            {
                _logger.LogError(ocrEx, $"OCR service failed: {ocrEx.Message}");
            }

            return "";
        }
    }
}

