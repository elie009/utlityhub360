namespace UtilityHub360.Controllers.PDFTextExtraction
{
    public class PDFExtractionBase
    {
        public virtual async Task<string> ExtractTextFromPDFAsync(Stream pdf)
        {
            return await Task.FromResult("Base PDF extraction not implemented.");
        }
    }
}

