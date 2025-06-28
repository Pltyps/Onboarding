using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MOAI.API.Models;
using MOAI.API.Data;

namespace MOAI.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly IConfiguration _config;

    public AuthController(ApplicationDbContext db, IConfiguration config)
    {
        _db = db;
        _config = config;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest req)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == req.Email);

        if (user == null || !BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash))
            return Unauthorized("Invalid credentials");

        // Set cookie manually
        HttpContext.Response.Cookies.Append("user_email", user.Email, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
            Path = "/",
            Expires = DateTimeOffset.UtcNow.AddHours(1)
        });
        HttpContext.Response.Cookies.Append("user_role", user.Role, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
            Path = "/",
            Expires = DateTimeOffset.UtcNow.AddHours(1)
        });
        HttpContext.Response.Cookies.Append("user_department", user.Department, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
            Path = "/",
            Expires = DateTimeOffset.UtcNow.AddHours(1)
        });

        return Ok(new
        {
            message = "Logged in",
            email = user.Email,
            role = user.Role,
            department = user.Department
        });

    }

    [HttpPost("logout")]
    public IActionResult Logout()
    {
        Response.Cookies.Delete("user_email");
        Response.Cookies.Delete("user_role");
        Response.Cookies.Delete("user_department");
        return Ok(new { message = "Logged out" });
    }

}
