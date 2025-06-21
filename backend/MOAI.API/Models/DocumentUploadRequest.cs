using MOAI.API.Validation;

namespace MOAI.API.Models;

/// <summary>
/// DTO used to receive upload request from the frontend.
/// Contains the file, department name, and uploader's identity.
/// </summary>
public class DocumentUploadRequest
{
    /// <summary>
    /// Uploaded file (.txt or .md only for now).
    /// </summary>
    [AllowedExtensions(new[] { ".txt", ".md", ".docx" })]
    public IFormFile File { get; set; } = default!;


}
