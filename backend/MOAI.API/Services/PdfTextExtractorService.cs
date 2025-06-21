namespace MOAI.API.Services
{
    public class PdfTextExtractorService
    {
        public Task<string> ExtractTextAsync(string pdfPath)
        {
            // TODO: Implement PDF-to-text later with PdfPig or another tool
            return Task.FromResult($"[Extracted text from {Path.GetFileName(pdfPath)}]");
        }
    }
}
