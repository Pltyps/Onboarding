using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using System.Text;

namespace MOAI.API.Utils
{
    public static class WordDocReader
    {
        public static string ReadToText(string path)
        {
            try
            {
                using var wordDoc = WordprocessingDocument.Open(path, false);
                var body = wordDoc.MainDocumentPart?.Document?.Body;

                if (body == null)
                    return "";

                var sb = new StringBuilder();

                foreach (var para in body.Descendants<Paragraph>())
                {
                    var text = para.InnerText.Trim();

                    // Skip empty or irrelevant content
                    if (string.IsNullOrWhiteSpace(text)) continue;
                    if (text.Contains("TOC", StringComparison.OrdinalIgnoreCase)) continue;
                    if (text.Contains("PAGEREF", StringComparison.OrdinalIgnoreCase)) continue;
                    if (text.Contains(@"\o", StringComparison.OrdinalIgnoreCase)) continue;

                    // Normalize tabs and weird spacing
                    text = System.Text.RegularExpressions.Regex.Replace(text, @"\s{2,}", " ");
                    sb.AppendLine(text);
                }

                var result = sb.ToString().Trim();
                Console.WriteLine($"[WordDocReader] Extracted {result.Length} characters from: {path}");
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WordDocReader] OpenXML ERROR: {ex.Message}");
                return "";
            }
        }
    }
}
