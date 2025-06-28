using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MOAI.API.Data;
using MOAI.API.Services; // for ActiveUserTracker
using MOAI.API.Models;

namespace MOAI.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public AdminController(ApplicationDbContext db)
    {
        _db = db;
    }

    [HttpGet("stats")]
    public IActionResult GetSystemStats()
    {
        return Ok(AdminStatsProvider.GetSnapshot());
    }
    // Drill into one session
    [HttpGet("chat/{id}")]
    public async Task<IActionResult> GetChatById(int id)
    {
        var session = await _db.ChatSessions
            .Include(s => s.Messages)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (session == null)
            return NotFound(new { error = "Chat not found." });

        return Ok(session);
    }

    // Get chat rating breakdown
    [HttpGet("feedback-summary")]
    public async Task<IActionResult> GetFeedbackSummary()
    {
        var helpful = await _db.ChatMessages.CountAsync(m => m.IsHelpful == true);
        var unhelpful = await _db.ChatMessages.CountAsync(m => m.IsHelpful == false);
        var unrated = await _db.ChatMessages.CountAsync(m => m.IsHelpful == null);

        return Ok(new
        {
            helpful,
            unhelpful,
            unrated,
            total = helpful + unhelpful + unrated
        });
    }

    // List all users who’ve chatted
    [HttpGet("users")]
    public async Task<IActionResult> GetDistinctUsers()
    {
        var users = await _db.ChatSessions
            .Select(c => c.UserEmail)
            .Distinct()
            .ToListAsync();

        return Ok(users);
    }

    private double GetUptimeMinutes()
    {
        var uptime = TimeSpan.FromMilliseconds(Environment.TickCount64);
        return Math.Round(uptime.TotalMinutes, 1);
    }
}
