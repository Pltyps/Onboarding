using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using System;
using System.Linq;
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
                    if (!string.IsNullOrWhiteSpace(text))
                        sb.AppendLine(text);
                }

                var result = sb.ToString();
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
