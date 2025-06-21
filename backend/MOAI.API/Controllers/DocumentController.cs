using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MOAI.API.Models;
using MOAI.API.Services;
using MOAI.API.Validation;

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

    // Upload Endpoint
    [Authorize(Roles = "Admin,FullTime")]
    [HttpPost("upload")]
    public async Task<IActionResult> Upload([FromForm] DocumentUploadRequest request)
    {
        var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
        var userDepartment = User.FindFirst("Department")?.Value;

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (request.File == null || request.File.Length == 0)
            return BadRequest("No file selected.");

        var fileName = Path.GetFileName(request.File.FileName);

        if (!FileValidationConstants.AllowedExtensions.Contains(Path.GetExtension(fileName).ToLowerInvariant()))
            return BadRequest("Only .txt, .md, .docx, or .pdf files are allowed.");

        var (isDuplicate, existingPdfPath) = await _docService.CheckForDuplicateAsync(fileName);
        if (isDuplicate)
        {
            return Ok(new
            {
                duplicate = true,
                fileName,
                existingPdf = existingPdfPath
            });
        }

        await _docService.SaveFileAsync(
            request.File,
            department: userDepartment ?? "Unknown",
            uploadedBy: userEmail ?? "unknown"
        );



        return Ok(new { duplicate = false, message = "Upload successful." });
    }

    // Get All files Endpoints
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

    // Get JSON object by file name Endpoint
    [Authorize]
    [HttpGet("{fileName}")]
    public async Task<IActionResult> GetByFileName(string fileName)
    {
        var doc = await _docService.GetByFileNameAsync(fileName);
        if (doc == null)
            return NotFound("Document not found");

        return Ok(doc);
    }

    // Get raw content (stream) by file name for iframe display Endpoint
    [Authorize]
    [HttpGet("view/{fileName}")]
    public async Task<IActionResult> View(string fileName)
    {
        var doc = await _docService.GetByFileNameAsync(fileName);
        if (doc == null || !System.IO.File.Exists(doc.PdfPath))
            return NotFound("Document or file missing.");

        return PhysicalFile(doc.PdfPath, "application/pdf", enableRangeProcessing: true);
    }


    // Delete Endpoint
    [Authorize(Roles = "Admin,FullTime")]
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
