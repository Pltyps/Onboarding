using Microsoft.AspNetCore.Http;
using MOAI.API.Models;

namespace MOAI.API.Services;

/// <summary>
/// Interface defining document-related business logic.
/// Useful for dependency injection and future testability.
/// </summary>
public interface IDocumentService
{
    /// <summary>
    /// Checks if a file with the same name already exists.
    /// </summary>
    Task<(bool IsDuplicate, string ExistingContent)> CheckForDuplicateAsync(string fileName);

    /// <summary>
    /// Extracts safe plain text content from the uploaded file.
    /// </summary>
    Task<string> ExtractSafeTextAsync(IFormFile file);

    /// <summary>
    /// Stores the uploaded file content, replacing older version if needed.
    /// </summary>
    Task SaveFileAsync(string fileName, string department, string content, string uploadedBy);

    Task<List<StoredDocument>> GetAllAsync();
    Task<StoredDocument?> GetByFileNameAsync(string fileName);

}
