using System.Security.Claims;
using MOAI.API.Data;
using MOAI.API.Models;
using MOAI.API.Services;
using Microsoft.EntityFrameworkCore;
using MOAI.API.Utils;
using Azure.Storage.Blobs;

public class DocumentService : IDocumentService
{
    private readonly ApplicationDbContext _db;
    private readonly IWebHostEnvironment _env;
    private readonly PdfConverterService _pdfConverter;
    private readonly IHttpContextAccessor _http;
    private readonly BlobContainerClient _blobClient;

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

        var connectionString = Environment.GetEnvironmentVariable("AZURE_STORAGE_CONNECTION_STRING")!;
        var containerName = Environment.GetEnvironmentVariable("AZURE_STORAGE_CONTAINER_NAME")!;
        _blobClient = new BlobContainerClient(connectionString, containerName);
    }

    public async Task<(bool IsDuplicate, string ExistingPath)> CheckForDuplicateAsync(string fileName)
    {
        var baseName = Path.GetFileNameWithoutExtension(fileName)
            .Trim()
            .ToLowerInvariant();
        var allDocs = await _db.Documents.AsNoTracking().ToListAsync();

        var existing = allDocs
            .OrderByDescending(d => d.UploadedAt)
            .FirstOrDefault(d =>
                Path.GetFileNameWithoutExtension(d.FileName)
                    .Trim()
                    .ToLowerInvariant() == baseName);

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
        string pdfPath;
        var outputDir = Path.Combine(_env.WebRootPath, "converted");
        Directory.CreateDirectory(outputDir);

        if (extension == ".docx")
        {
            var rawText = WordDocReader.ReadToText(tempPath);
            extractedText = FileSanitizer.Sanitize(rawText);

            var targetPdfPath = Path.Combine(outputDir, Path.ChangeExtension(originalFileName, ".pdf"));
            pdfPath = _pdfConverter.ConvertToPdf(tempPath, targetPdfPath);
        }
        else if (extension == ".txt" || extension == ".md")
        {
            extractedText = await File.ReadAllTextAsync(tempPath);
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

        var blobName = Path.GetFileName(pdfPath);
        var blobClient = _blobClient.GetBlobClient(blobName);
        await using var blobStream = File.OpenRead(pdfPath);
        await blobClient.UploadAsync(blobStream, overwrite: true);
        var blobUri = blobClient.Uri.ToString();

        System.IO.File.Delete(pdfPath); // delete local PDF after upload

        var baseName = Path.GetFileNameWithoutExtension(originalFileName)
            .Trim()
            .ToLowerInvariant();

        var allDocs = await _db.Documents.ToListAsync();
        var existing = allDocs
            .OrderByDescending(d => d.UploadedAt)
            .FirstOrDefault(d =>
                Path.GetFileNameWithoutExtension(d.FileName)
                    .Trim()
                    .ToLowerInvariant() == baseName);


        StoredDocument doc;
        if (existing != null)
        {
            existing.FileName = originalFileName;
            existing.Department = department;
            existing.UploadedAt = DateTime.UtcNow;
            existing.Content = extractedText ?? "";
            existing.PdfPath = blobUri;

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
                PdfPath = blobUri,
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
        var normalizedBase = Path.GetFileNameWithoutExtension(Uri.UnescapeDataString(fileName))
            .Trim()
            .ToLowerInvariant();

        return await _db.Documents
            .AsNoTracking()
            .OrderByDescending(d => d.UploadedAt)
            .FirstOrDefaultAsync(d =>
                Path.GetFileNameWithoutExtension(d.FileName)
                    .Trim()
                    .ToLowerInvariant() == normalizedBase);
    }




    public async Task<bool> DeleteFileAsync(string fileName)
    {
        try
        {
            if (Path.GetFileName(fileName) != fileName)
                return false;

            var doc = await _db.Documents.FirstOrDefaultAsync(d => d.FileName == fileName);
            if (doc == null) return false;

            var user = _http.HttpContext?.User;
            var userRole = user?.FindFirst(ClaimTypes.Role)?.Value ?? "";
            var userEmail = user?.FindFirst(ClaimTypes.Email)?.Value ?? "";
            var isAdmin = userRole == "Admin";
            var isUploader = userRole == "FullTime" && userEmail == doc.UploadedBy;

            if (!isAdmin && !isUploader) return false;

            // 🧹 Optional: delete from Azure Blob Storage
            var blobName = Path.GetFileName(doc.PdfPath);
            var blob = _blobClient.GetBlobClient(blobName);
            await blob.DeleteIfExistsAsync();

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
