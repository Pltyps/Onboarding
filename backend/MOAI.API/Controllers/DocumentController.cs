using Microsoft.AspNetCore.Mvc;
using MOAI.API.Models;
using MOAI.API.Services;

namespace MOAI.API.Controllers;

/// <summary>
/// API Controller to handle document upload, duplication checks, and retrieval.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class DocumentController : ControllerBase
{
    private readonly IDocumentService _docService;

    public DocumentController(IDocumentService docService)
    {
        _docService = docService;
    }

    /// <summary>
    /// Handles a file upload request.
    /// - Extracts plaintext from the uploaded file (.txt or .md)
    /// - Checks for existing file with same name
    /// - If a duplicate is found, returns both versions for diffing (handled client-side)
    /// - If no duplicate, saves the file to the database
    /// </summary>
    /// <param name="request">Upload request object with file, department, and uploader info</param>
    /// <returns>HTTP response with upload result or diff preview</returns>
    [HttpPost("upload")]
    public async Task<IActionResult> Upload([FromForm] DocumentUploadRequest request)
    {
        // 🔒 Future: replace this logic with [Authorize(Roles = "Admin")]
        if (request.UploadedBy.ToLower() != "admin@byu.edu")
            return Forbid("Only Admins are allowed to upload documents.");

        // Validate file presence
        if (request.File == null || request.File.Length == 0)
            return BadRequest("No file selected.");

        string sanitizedText;
        try
        {
            // Extract safe, plain text (rejects unsupported extensions)
            sanitizedText = await _docService.ExtractSafeTextAsync(request.File);
        }
        catch (InvalidOperationException ex)
        {
            // Return error if file type is invalid
            return BadRequest(ex.Message);
        }

        // Check if a file with the same name already exists
        var (isDuplicate, existingContent) = await _docService.CheckForDuplicateAsync(request.File.FileName);

        if (isDuplicate)
        {
            // Return both versions to the frontend so it can display a diff UI
            return Ok(new
            {
                duplicate = true,
                existingContent,
                uploadedContent = sanitizedText,
                fileName = request.File.FileName
            });
        }

        // Save file content into database if no duplicate
        await _docService.SaveFileAsync(
            fileName: request.File.FileName,
            department: request.Department,
            content: sanitizedText,
            uploadedBy: request.UploadedBy
        );

        return Ok(new
        {
            duplicate = false,
            message = "Upload successful."
        });
    }

    /// <summary>
    /// Returns a full list of all stored documents.
    /// - Used to display a document library or index
    /// 🔧 You may exclude Content in the future if the payload is too large.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var docs = await _docService.GetAllAsync();
        return Ok(docs);
    }

    /// <summary>
    /// Retrieves a specific stored document by filename.
    /// - Used to render the document in the frontend or allow Chatbot to reference it
    /// </summary>
    /// <param name="fileName">The exact name of the file (case-sensitive)</param>
    /// <returns>HTTP 200 with the document if found; 404 otherwise</returns>
    [HttpGet("{fileName}")]
    public async Task<IActionResult> GetByFileName(string fileName)
    {
        var doc = await _docService.GetByFileNameAsync(fileName);

        if (doc == null)
            return NotFound("Document not found");

        return Ok(doc);
    }
}
