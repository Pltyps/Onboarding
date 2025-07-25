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

        public string ConvertToPdf(string inputPath, string outputPath)
        {
            var outputDir = Path.GetDirectoryName(outputPath)!;
            Directory.CreateDirectory(outputDir);

            Console.WriteLine($"[PDF] input: {inputPath}");
            Console.WriteLine($"[PDF] outputDir: {outputDir}");
            Console.WriteLine($"[PDF] expecting: {outputPath}");

            var psi = new ProcessStartInfo
            {
                FileName = "libreoffice",  // 🔁 <-- this is the key fix
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

            if (!File.Exists(outputPath))
            {
                throw new Exception("PDF conversion failed: " + outputPath);
            }

            return outputPath;
        }




        public string ConvertHtmlToPdf(string html, string outputPath)
        {
            var outputDir = Path.GetDirectoryName(outputPath)!;
            Directory.CreateDirectory(outputDir);

            var htmlPath = Path.Combine(Path.GetTempPath(), Path.GetFileNameWithoutExtension(outputPath) + ".html");
            File.WriteAllText(htmlPath, html);

            var psi = new ProcessStartInfo
            {
                FileName = "wkhtmltopdf",
                Arguments = $"\"{htmlPath}\" \"{outputPath}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            var process = Process.Start(psi);
            process?.WaitForExit();

            File.Delete(htmlPath);

            if (!File.Exists(outputPath))
                throw new Exception("HTML to PDF conversion failed: " + outputPath);

            return outputPath;
        }

    }
}
