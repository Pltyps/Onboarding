using System.Security.Claims;
using MOAI.API.Data;
using MOAI.API.Models;
using MOAI.API.Services;
using Microsoft.EntityFrameworkCore;
using MOAI.API.Utils;

public class DocumentService : IDocumentService
{
    private readonly ApplicationDbContext _db;
    private readonly IWebHostEnvironment _env;
    private readonly PdfConverterService _pdfConverter;
    private readonly IHttpContextAccessor _http;

    public DocumentService(
        ApplicationDbContext db,
        IWebHostEnvironment env,
        PdfConverterService pdfConverter,
        IHttpContextAccessor http)
    {
        _db = db;
        _env = env;
        _pdfConverter = pdfConverter;
        _http = http;
    }

    public async Task<(bool IsDuplicate, string ExistingPath)> CheckForDuplicateAsync(string fileName)
    {
        var baseName = Path.GetFileNameWithoutExtension(fileName);
        var allDocs = await _db.Documents.AsNoTracking().ToListAsync();

        var existing = allDocs
            .OrderByDescending(d => d.UploadedAt)
            .FirstOrDefault(d => Path.GetFileNameWithoutExtension(d.FileName) == baseName);

        return (existing != null, existing?.PdfPath ?? string.Empty);
    }

    public async Task SaveFileAsync(IFormFile file, string department, string uploadedBy)
    {
        var originalFileName = Path.GetFileName(file.FileName);
        var extension = Path.GetExtension(originalFileName).ToLowerInvariant();
        var tempPath = Path.Combine(Path.GetTempPath(), originalFileName);

        using (var stream = new FileStream(tempPath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        string? extractedText = null;

        if (extension == ".txt" || extension == ".md")
        {
            extractedText = await File.ReadAllTextAsync(tempPath);
        }
        else if (extension == ".docx")
        {
            extractedText = WordDocReader.ReadToText(tempPath);
        }

        string pdfPath;
        var outputDir = Path.Combine(_env.WebRootPath, "converted");
        Directory.CreateDirectory(outputDir);

        if (extension == ".docx")
        {
            var targetPdfPath = Path.Combine(outputDir, Path.ChangeExtension(originalFileName, ".pdf"));
            pdfPath = _pdfConverter.ConvertToPdf(tempPath, targetPdfPath);
        }
        else if (extension == ".txt" || extension == ".md")
        {
            var html = $"<pre>{System.Net.WebUtility.HtmlEncode(extractedText ?? "")}</pre>";
            var targetPdfPath = Path.Combine(outputDir, Path.ChangeExtension(originalFileName, ".pdf"));
            pdfPath = _pdfConverter.ConvertHtmlToPdf(html, targetPdfPath);
        }

        else if (extension == ".pdf")
        {
            pdfPath = Path.Combine(outputDir, originalFileName);
            System.IO.File.Copy(tempPath, pdfPath, overwrite: true);
        }
        else
        {
            throw new InvalidOperationException("Unsupported file type.");
        }

        System.IO.File.Delete(tempPath);

        var baseName = Path.GetFileNameWithoutExtension(originalFileName);
        var allDocs = await _db.Documents.ToListAsync();
        var existing = allDocs
            .OrderByDescending(d => d.UploadedAt)
            .FirstOrDefault(d => Path.GetFileNameWithoutExtension(d.FileName) == baseName);

        StoredDocument doc;
        if (existing != null)
        {
            existing.FileName = originalFileName;
            existing.Department = department;
            existing.UploadedAt = DateTime.UtcNow;
            existing.PdfPath = pdfPath;
            existing.Content = extractedText ?? "";

            if (string.IsNullOrWhiteSpace(existing.UploadedBy))
            {
                existing.UploadedBy = uploadedBy;
            }

            doc = existing;
        }
        else
        {
            doc = new StoredDocument
            {
                FileName = originalFileName,
                Department = department,
                UploadedBy = uploadedBy,
                UploadedAt = DateTime.UtcNow,
                PdfPath = pdfPath,
                Content = extractedText ?? ""
            };

            _db.Documents.Add(doc);
        }

        await _db.SaveChangesAsync();
    }

    public async Task<List<StoredDocument>> GetAllAsync()
    {
        return await _db.Documents.OrderBy(d => d.FileName).ToListAsync();
    }

    public async Task<StoredDocument?> GetByFileNameAsync(string fileName)
    {
        var baseName = Path.GetFileNameWithoutExtension(fileName);
        var allDocs = await _db.Documents.ToListAsync();

        return allDocs
            .OrderByDescending(d => d.UploadedAt)
            .FirstOrDefault(d => Path.GetFileNameWithoutExtension(d.FileName) == baseName);
    }

    public async Task<bool> DeleteFileAsync(string fileName)
    {
        try
        {
            if (Path.GetFileName(fileName) != fileName)
                return false;

            var doc = await _db.Documents.FirstOrDefaultAsync(d => d.FileName == fileName);
            if (doc == null)
                return false;

            var user = _http.HttpContext?.User;
            var userRole = user?.FindFirst(ClaimTypes.Role)?.Value ?? "";

            var isAdmin = userRole == "Admin";

            var userEmail = user?.FindFirst(ClaimTypes.Email)?.Value ?? "";
            var isUploadEmp = userRole == "FullTime" && userEmail == doc.UploadedBy;

            Console.WriteLine($"[Delete] Role: {userRole}, Allowed: {isAdmin || isUploadEmp}");

            if (!isAdmin && !isUploadEmp)
                return false;

            _db.Documents.Remove(doc);
            await _db.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }
}
