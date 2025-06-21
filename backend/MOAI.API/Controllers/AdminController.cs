using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MOAI.API.Services; // for ActiveUserTracker

namespace MOAI.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    [HttpGet("stats")]
    public IActionResult GetSystemStats()
    {
        return Ok(AdminStatsProvider.GetSnapshot());
    }


    private double GetUptimeMinutes()
    {
        var uptime = TimeSpan.FromMilliseconds(Environment.TickCount64);
        return Math.Round(uptime.TotalMinutes, 1);
    }
}
