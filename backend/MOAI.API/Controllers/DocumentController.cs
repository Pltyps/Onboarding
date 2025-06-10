using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using MOAI.API.Models;
using MOAI.API.Services;

namespace MOAI.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DocumentController : ControllerBase
{
    private readonly IDocumentService _docService;

    public DocumentController(IDocumentService docService)
    {
        _docService = docService;
    }

    [Authorize(Roles = "Admin,FullTime")]
    [HttpPost("upload")]
    public async Task<IActionResult> Upload([FromForm] DocumentUploadRequest request)
    {
        var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
        var userDepartment = User.FindFirst("Department")?.Value;

        if (request.File == null || request.File.Length == 0)
            return BadRequest("No file selected.");

        string fileName = Path.GetFileName(request.File.FileName);
        string sanitizedText;

        try
        {
            sanitizedText = await _docService.ExtractSafeTextAsync(request.File);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }

        var (isDuplicate, existingContent) = await _docService.CheckForDuplicateAsync(fileName);
        if (isDuplicate)
        {
            return Ok(new
            {
                duplicate = true,
                existingContent,
                uploadedContent = sanitizedText,
                fileName
            });
        }

        await _docService.SaveFileAsync(
            fileName: fileName,
            department: userDepartment ?? "Unknown",
            content: sanitizedText,
            uploadedBy: userEmail ?? "unknown"
        );

        return Ok(new { duplicate = false, message = "Upload successful." });
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var role = User.FindFirst(ClaimTypes.Role)?.Value;
        var dept = User.FindFirst("Department")?.Value;

        var docs = await _docService.GetAllAsync();

        if (role != "Admin")
            docs = docs.Where(d => d.Department == dept).ToList();

        return Ok(docs);
    }

    [Authorize]
    [HttpGet("{fileName}")]
    public async Task<IActionResult> GetByFileName(string fileName)
    {
        var doc = await _docService.GetByFileNameAsync(fileName);
        if (doc == null)
            return NotFound("Document not found");

        return Ok(doc);
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("delete/{fileName}")]
    public async Task<IActionResult> DeleteDocument(string fileName)
    {
        fileName = Path.GetFileName(fileName);

        var success = await _docService.DeleteFileAsync(fileName);
        if (!success)
            return NotFound("File not found or deletion failed.");

        return Ok(new { message = "File deleted successfully." });
    }
}
