using System;
using Microsoft.EntityFrameworkCore;
using MOAI.API.Data;
using MOAI.API.Models;
using Microsoft.AspNetCore.Hosting;

namespace MOAI.API.Services;

// Service handles document-related logic
public class DocumentService : IDocumentService
{
    private readonly ApplicationDbContext _db;
    private readonly IWebHostEnvironment _env;

    public DocumentService(ApplicationDbContext db, IWebHostEnvironment env)
    {
        _db = db;
        _env = env;
    }


    // Check if a document with the same filename already exists
    public async Task<(bool IsDuplicate, string ExistingContent)> CheckForDuplicateAsync(string fileName)
    {
        var existing = await _db.Documents.FirstOrDefaultAsync(d => d.FileName == fileName);
        return (existing != null, existing?.Content ?? string.Empty);
    }

    // Extracts safe plaintext from the uploaded file
    public async Task<string> ExtractSafeTextAsync(IFormFile file)
    {
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

        if (extension != ".txt" && extension != ".md" && extension != ".docx")
            throw new InvalidOperationException("Unsupported file type. Only .txt, .md, and .docx are allowed.");

        using var reader = new StreamReader(file.OpenReadStream());
        return await reader.ReadToEndAsync();
    }

    // Saves or overwrites an existing file with new content
    public async Task SaveFileAsync(string fileName, string department, string content, string uploadedBy)
    {
        var existing = await _db.Documents.FirstOrDefaultAsync(d => d.FileName == fileName);

        if (existing != null)
        {
            existing.Content = content;
            existing.Department = department;
            existing.UploadedBy = uploadedBy;
            existing.UploadedAt = DateTime.UtcNow;
        }
        else
        {
            var newDoc = new StoredDocument
            {
                FileName = fileName,
                Department = department,
                Content = content,
                UploadedBy = uploadedBy,
                UploadedAt = DateTime.UtcNow
            };

            _db.Documents.Add(newDoc);
        }

        await _db.SaveChangesAsync();
    }


    public async Task<List<StoredDocument>> GetAllAsync()
    {
        return await _db.Documents.OrderBy(d => d.FileName).ToListAsync();
    }

    public async Task<StoredDocument?> GetByFileNameAsync(string fileName)
    {
        return await _db.Documents.FirstOrDefaultAsync(d => d.FileName == fileName);
    }

    public async Task<bool> DeleteFileAsync(string fileName)
    {
        try
        {
            if (Path.GetFileName(fileName) != fileName) // stronger path sanitization
                return false;

            var doc = await _db.Documents.FirstOrDefaultAsync(d => d.FileName == fileName);
            if (doc == null)
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
