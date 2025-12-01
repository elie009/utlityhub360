using System.Text;
using System.Linq;
using UglyToad.PdfPig;

namespace UtilityHub360.Controllers.PDFTextExtraction
{
    public class ServicePdfPigBased : PDFExtractionBase
    {
        private readonly ILogger<ServicePdfPigBased> _logger;

        public ServicePdfPigBased(ILogger<ServicePdfPigBased> logger)
        {
            _logger = logger;
        }

        public override async Task<string> ExtractTextFromPDFAsync(Stream pdf)
        {
            try
            {
                pdf.Position = 0;
                var textBuilder = new StringBuilder();
                
                using (var document = PdfDocument.Open(pdf))
                {
                    var pageCount = document.NumberOfPages;
                    _logger.LogInformation($"PDF has {pageCount} pages");
                    
                    foreach (var page in document.GetPages())
                    {
                        try
                        {
                            var words = page.GetWords();
                            if (words != null && words.Any())
                            {
                                _logger.LogInformation($"Page {page.Number}: Found {words.Count()} words");
                                
                                // Extract all words without filtering - removed English-only filter to support currency symbols and international characters
                                var allWords = words
                                    .Where(w => !string.IsNullOrWhiteSpace(w.Text))
                                    .Select(w => w.Text);
                                
                                if (allWords.Any())
                                {
                                    var pageText = string.Join(" ", allWords);
                                    if (!string.IsNullOrWhiteSpace(pageText))
                                    {
                                        textBuilder.AppendLine(pageText);
                                        _logger.LogInformation($"Extracted {pageText.Length} characters from page {page.Number} ({allWords.Count()} words)");
                                    }
                                    else
                                    {
                                        _logger.LogWarning($"Page {page.Number}: Words found but joined text is empty");
                                    }
                                }
                                else
                                {
                                    _logger.LogWarning($"Page {page.Number}: All {words.Count()} words were empty or whitespace");
                                }
                            }
                            else
                            {
                                _logger.LogWarning($"Page {page.Number}: No words found (words is null or empty)");
                            }
                        }
                        catch (Exception pageEx)
                        {
                            _logger.LogWarning($"Error extracting text from PDF page {page.Number}: {pageEx.Message}");
                        }
                    }
                }
                
                var extractedText = textBuilder.ToString();
                if (!string.IsNullOrWhiteSpace(extractedText))
                {
                    _logger.LogInformation($"Successfully extracted {extractedText.Length} characters from PDF using PdfPig");
                    return extractedText;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"PdfPig extraction failed: {ex.Message}");
            }

            return string.Empty;
        }
    }
}

