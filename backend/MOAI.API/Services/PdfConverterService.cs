using System.Diagnostics;

namespace MOAI.API.Services
{
    public class PdfConverterService
    {
        private readonly IWebHostEnvironment _env;

        public PdfConverterService(IWebHostEnvironment env)
        {
            _env = env;
        }

        public string ConvertToPdf(string inputPath)
        {
            var outputDir = Path.Combine(_env.WebRootPath, "converted");
            Directory.CreateDirectory(outputDir);

            var psi = new ProcessStartInfo
            {
                FileName = "soffice",
                Arguments = $"--headless --convert-to pdf \"{inputPath}\" --outdir \"{outputDir}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            var process = Process.Start(psi);
            process?.WaitForExit();

            var output = process?.StandardOutput.ReadToEnd();
            var error = process?.StandardError.ReadToEnd();
            Console.WriteLine("LibreOffice output: " + output);
            Console.WriteLine("LibreOffice error: " + error);

            var outputPdf = Path.Combine(outputDir, Path.GetFileNameWithoutExtension(inputPath) + ".pdf");

            if (!File.Exists(outputPdf))
            {
                Console.WriteLine($"Resolved output directory: {outputDir}");
                throw new Exception("PDF conversion failed: " + outputPdf);
            }

            return outputPdf;
        }


        public string ConvertHtmlToPdf(string html, string fileName)
        {
            var outputDir = Path.Combine(_env.WebRootPath, "converted");
            Directory.CreateDirectory(outputDir);

            var htmlPath = Path.Combine(Path.GetTempPath(), Path.GetFileNameWithoutExtension(fileName) + ".html");
            var pdfPath = Path.Combine(outputDir, Path.GetFileNameWithoutExtension(fileName) + ".pdf");

            File.WriteAllText(htmlPath, html);

            var psi = new ProcessStartInfo
            {
                FileName = "wkhtmltopdf",
                Arguments = $"\"{htmlPath}\" \"{pdfPath}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            var process = Process.Start(psi);
            process?.WaitForExit();

            File.Delete(htmlPath);

            if (!File.Exists(pdfPath))
                throw new Exception("HTML to PDF conversion failed: " + pdfPath);

            return pdfPath;
        }
    }
}
