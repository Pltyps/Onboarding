using MOAI.API.Models;

namespace MOAI.API.Services;

/// <summary>
/// Interface defining document-related business logic.
/// Useful for dependency injection and future testability.
/// </summary>
public interface IDocumentService
{
    Task SaveFileAsync(IFormFile file, string department, string uploadedBy);

    Task<(bool IsDuplicate, string ExistingPath)> CheckForDuplicateAsync(string fileName);

    Task<List<StoredDocument>> GetAllAsync();
    Task<StoredDocument?> GetByFileNameAsync(string fileName);
    Task<bool> DeleteFileAsync(string fileName);
}
