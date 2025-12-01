using System.Text;
using System.Linq;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

namespace UtilityHub360.Controllers.PDFTextExtraction
{
    /// <summary>
    /// Alternative PDF text extraction service using PdfPig with different extraction methods
    /// This is used as a fallback when standard PdfPig extraction fails
    /// </summary>
    public class ServicePdfPigAlternative : PDFExtractionBase
    {
        private readonly ILogger<ServicePdfPigAlternative> _logger;

        public ServicePdfPigAlternative(ILogger<ServicePdfPigAlternative> logger)
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
                    _logger.LogInformation($"Alternative PDF extraction: PDF has {pageCount} pages");
                    
                    foreach (var page in document.GetPages())
                    {
                        try
                        {
                            // Method 1: Try extracting words without filtering
                            try
                            {
                                var words = page.GetWords();
                                if (words != null && words.Any())
                                {
                                    _logger.LogInformation($"Alternative extraction - Page {page.Number}: Found {words.Count()} words");
                                    
                                    // Extract all words without any filtering
                                    var allWords = words
                                        .Where(w => !string.IsNullOrWhiteSpace(w.Text))
                                        .Select(w => w.Text)
                                        .ToList();
                                    
                                    if (allWords.Any())
                                    {
                                        var pageText = string.Join(" ", allWords);
                                        if (!string.IsNullOrWhiteSpace(pageText))
                                        {
                                            textBuilder.AppendLine(pageText);
                                            _logger.LogInformation($"Alternative extraction: Extracted {pageText.Length} characters from page {page.Number} using unfiltered words ({allWords.Count()} words)");
                                            continue; // Success, move to next page
                                        }
                                        else
                                        {
                                            _logger.LogWarning($"Alternative extraction - Page {page.Number}: Words found but joined text is empty");
                                        }
                                    }
                                    else
                                    {
                                        _logger.LogWarning($"Alternative extraction - Page {page.Number}: All {words.Count()} words were empty or whitespace");
                                    }
                                }
                                else
                                {
                                    _logger.LogWarning($"Alternative extraction - Page {page.Number}: No words found (words is null or empty)");
                                }
                            }
                            catch (Exception wordsEx)
                            {
                                _logger.LogWarning($"Alternative extraction - Page {page.Number}: Unfiltered words extraction failed: {wordsEx.Message}");
                            }

                            // Method 2: Try to extract text using letters (more granular than words)
                            // This might work when GetWords() fails due to word boundaries or formatting issues
                            try
                            {
                                var letters = page.Letters;
                                if (letters != null && letters.Any())
                                {
                                    _logger.LogInformation($"Alternative extraction - Page {page.Number}: Found {letters.Count()} letters");
                                    
                                    // Join letters directly
                                    var pageText = string.Join("", letters.Select(l => l.Value));
                                    if (!string.IsNullOrWhiteSpace(pageText))
                                    {
                                        textBuilder.AppendLine(pageText);
                                        _logger.LogInformation($"Alternative extraction: Extracted {pageText.Length} characters from page {page.Number} using letters");
                                        continue; // Success, move to next page
                                    }
                                    else
                                    {
                                        _logger.LogWarning($"Alternative extraction - Page {page.Number}: Letters found but joined text is empty");
                                    }
                                }
                                else
                                {
                                    _logger.LogWarning($"Alternative extraction - Page {page.Number}: No letters found (letters is null or empty)");
                                }
                            }
                            catch (Exception lettersEx)
                            {
                                _logger.LogWarning($"Alternative extraction - Page {page.Number}: Letters extraction failed: {lettersEx.Message}");
                            }
                        }
                        catch (Exception pageEx)
                        {
                            _logger.LogWarning($"Error in alternative extraction from PDF page {page.Number}: {pageEx.Message}");
                        }
                    }
                }
                
                var extractedText = textBuilder.ToString();
                if (!string.IsNullOrWhiteSpace(extractedText))
                {
                    _logger.LogInformation($"Alternative extraction: Successfully extracted {extractedText.Length} characters from PDF using PdfPig alternative methods");
                    return extractedText;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Alternative PdfPig extraction failed: {ex.Message}");
            }

            return string.Empty;
        }
    }
}

