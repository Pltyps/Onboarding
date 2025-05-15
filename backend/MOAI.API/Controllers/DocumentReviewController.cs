using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MOAI.API.Data;

namespace MOAI.API.Controllers;

/// <summary>
/// Optional controller to support document review (e.g., for future approval workflows).
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class DocumentReviewController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public DocumentReviewController(ApplicationDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Returns documents that have not been approved yet.
    /// 🔧 Only needed if you implement an approval flow.
    /// </summary>
    [HttpGet("pending")]
    public async Task<IActionResult> GetUnapproved()
    {
        var pendingDocs = await _db.Documents
            .Where(d => !d.Approved)
            .ToListAsync();

        return Ok(pendingDocs);
    }
}
