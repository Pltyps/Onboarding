using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MOAI.API.Models;
using MOAI.API.Services;

namespace MOAI.API.Controllers
{
    [ApiController]
    [Route("api/chat")]
    [Authorize]
    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatService;

        public ChatController(IChatService chatService)
        {
            _chatService = chatService;
        }

        /// <summary>
        /// POST /api/chat?stream=true   → Server-sent-events (text/event-stream)
        /// POST /api/chat               → JSON { reply: "…" }
        /// </summary>
        [HttpPost, DisableRequestSizeLimit]
        public async Task<IActionResult> Chat(
            [FromBody] ChatRequest req,
            [FromQuery] bool stream = false,
            CancellationToken ct = default
        )
        {
            var iden = HttpContext.User.Identity as ClaimsIdentity;
            var user = new AppUser
            {
                Email = iden?.FindFirst(ClaimTypes.Email)?.Value ?? "",
                Role = iden?.FindFirst(ClaimTypes.Role)?.Value ?? "",
                Department = iden?.FindFirst("Department")?.Value ?? ""
            };

            if (stream)
            {
                Response.ContentType = "text/event-stream";
                await foreach (var token in _chatService.StreamChatAsync(user, req.Message, ct))
                {
                    await Response.WriteAsync(token);
                    await Response.Body.FlushAsync(ct);
                }
                return new EmptyResult();
            }
            else
            {
                // now matches the new signature
                var reply = await _chatService.GenerateResponseAsync(req.Message, user, ct);
                return Ok(new { reply });
            }
        }
    }
}
