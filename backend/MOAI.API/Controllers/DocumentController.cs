using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MOAI.API.Models;
using MOAI.API.Services;
using MOAI.API.Utils;
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
    public async Task<IActionResult> View(
        string fileName,
        [FromServices] IDocumentService docService,
        [FromServices] AzureBlobService blobService)
    {
        var doc = await docService.GetByFileNameAsync(fileName);
        if (doc == null || string.IsNullOrWhiteSpace(doc.PdfPath))
            return NotFound("Document not found");

        var blobName = Path.GetFileName(doc.PdfPath); // just the filename
        var stream = await blobService.DownloadFileAsync(blobName);
        if (stream == null)
            return NotFound("File not found in blob storage.");

        return File(stream, "application/pdf", enableRangeProcessing: true);
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

    // Temporary preview endpoint for .docx files on upload before being confirmed to be committed to the database
    [Authorize(Roles = "Admin,FullTime")]
    [HttpPost("preview")]
    public async Task<IActionResult> ExtractPreview([FromForm] IFormFile file)
    {
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (ext != ".docx")
            return BadRequest("Only .docx preview is supported.");

        var tempPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

        try
        {
            using (var stream = new FileStream(tempPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var extracted = WordDocReader.ReadToText(tempPath);
            return Ok(new { content = extracted });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error reading file: {ex.Message}");
        }
        finally
        {
            if (System.IO.File.Exists(tempPath))
                System.IO.File.Delete(tempPath);
        }
    }

}
