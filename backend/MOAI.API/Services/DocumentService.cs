using MOAI.API.Data;
using MOAI.API.Models;
using Microsoft.EntityFrameworkCore;

namespace MOAI.API.Services;

// Service handles document-related logic
public class DocumentService : IDocumentService
{
    private readonly ApplicationDbContext _db;

    public DocumentService(ApplicationDbContext db)
    {
        _db = db;
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
        // 🔧 Supports only .txt and .md for now; extend this if you add support for .docx with conversion
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (extension != ".txt" && extension != ".md")
            throw new InvalidOperationException("Unsupported file type. Only .txt and .md are allowed.");

        using var reader = new StreamReader(file.OpenReadStream());
        return await reader.ReadToEndAsync();
    }

    // Saves or overwrites an existing file with new content
    public async Task SaveFileAsync(string fileName, string department, string content, string uploadedBy)
    {
        // Remove any existing version of the file (based on filename match)
        var existing = await _db.Documents.FirstOrDefaultAsync(d => d.FileName == fileName);
        if (existing != null)
        {
            _db.Documents.Remove(existing);
        }

        var newDoc = new StoredDocument
        {
            FileName = fileName,
            Department = department,
            Content = content,
            UploadedBy = uploadedBy,
            UploadedAt = DateTime.UtcNow
        };

        _db.Documents.Add(newDoc);
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

}
