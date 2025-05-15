namespace MOAI.API.Models;

/// <summary>
/// Represents a sanitized, versioned text document stored in the database.
/// </summary>
public class StoredDocument
{
    public int Id { get; set; } // Primary Key

    /// <summary>
    /// File name (must be unique per upload group — used for duplicate checking).
    /// 🔧 Used to detect overwrite scenarios.
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// Logical grouping (e.g., IT, HR, Admissions).
    /// 🔧 Displayed on frontend and used for categorization.
    /// </summary>
    public string Department { get; set; } = string.Empty;

    /// <summary>
    /// Clean, readable plaintext content from the uploaded file.
    /// Stored here instead of binary file.
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Email or username of the uploader (for auditing).
    /// </summary>
    public string UploadedBy { get; set; } = string.Empty;

    /// <summary>
    /// Timestamp (UTC) of when the file was uploaded or replaced.
    /// </summary>
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    public bool Approved { get; set; } = true; // Default to true unless review is added

}
