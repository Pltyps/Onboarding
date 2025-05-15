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
    public IFormFile File { get; set; } = default!;

    /// <summary>
    /// Department or unit the file is related to.
    /// 🔧 Make sure this maps to a valid frontend display category.
    /// </summary>
    public string Department { get; set; } = string.Empty;

    /// <summary>
    /// Username or identifier of uploader.
    /// 🔧 In production, this should come from a validated user token.
    /// </summary>
    public string UploadedBy { get; set; } = string.Empty;
}
