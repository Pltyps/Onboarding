using Microsoft.AspNetCore.Mvc;
using MOAI.API.Models;
using MOAI.API.Data;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;

namespace MOAI.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public AuthController(ApplicationDbContext db) => _db = db;

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromForm] string email, [FromForm] string password)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            return Unauthorized("Invalid credentials");

        // 🔧 You may add JWT here later; for now return role info for frontend testing
        return Ok(new { user.Email, user.Role });
    }
}
