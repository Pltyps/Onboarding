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
        private readonly ChatHistoryService _history;

        public ChatController(IChatService chatService, ChatHistoryService history)
        {
            _chatService = chatService;
            _history = history;
        }

        private AppUser GetAppUser()
        {
            var iden = HttpContext.User.Identity as ClaimsIdentity;
            return new AppUser
            {
                Email = iden?.FindFirst(ClaimTypes.Email)?.Value ?? "",
                Role = iden?.FindFirst(ClaimTypes.Role)?.Value ?? "",
                Department = iden?.FindFirst("Department")?.Value ?? ""
            };
        }

        private string GetUserEmail()
        {
            return User.FindFirst(ClaimTypes.Email)?.Value ?? throw new UnauthorizedAccessException("Missing user email claim.");
        }

        // POST /api/chat?stream=true
        [HttpPost]
        [DisableRequestSizeLimit]
        public async Task<IActionResult> Chat(
            [FromBody] ChatRequest req,
            [FromQuery] bool stream = false,
            CancellationToken ct = default)
        {
            var user = GetAppUser();

            if (req.IsFirstMessage)
            {
                var title = req.Message.Length > 40
                    ? req.Message.Substring(0, 40) + "..."
                    : req.Message;

                await _history.UpdateTitleAsync(req.ChatSessionId, title);
            }

            if (stream)
            {
                Response.ContentType = "text/event-stream";
                await foreach (var token in _chatService.StreamChatAsync(user, req.Message, req.ChatSessionId, ct))
                {
                    await Response.WriteAsync(token);
                    await Response.Body.FlushAsync(ct);
                }
                return new EmptyResult();
            }
            else
            {
                var result = await _chatService.GenerateResponseAsync(req.Message, user, req.ChatSessionId, ct);
                return Content(result, "application/json");

            }
        }


        // POST /api/chat/new
        [HttpPost("new")]
        public async Task<IActionResult> StartNewChat([FromBody] string? title)
        {
            var email = GetUserEmail();

            try
            {
                var chat = await _history.CreateNewChatAsync(email, title);
                return Ok(chat);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        // GET /api/chat
        [HttpGet]
        public async Task<IActionResult> GetChats()
        {
            var email = GetUserEmail();
            var chats = await _history.GetChatsForUserAsync(email);
            return Ok(chats);
        }

        // DELETE /api/chat/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteChat(int id)
        {
            var email = GetUserEmail();
            var success = await _history.DeleteChatAsync(id, email);

            if (!success)
                return NotFound(new { error = "Chat not found or does not belong to user." });

            return NoContent();
        }

        // GET /api/chat/{id}/messages
        [HttpGet("{id}/messages")]
        public async Task<IActionResult> GetMessages(int id)
        {
            try
            {
                var email = GetUserEmail();
                var chats = await _history.GetChatsForUserAsync(email);

                if (!chats.Any(c => c.Id == id))
                    return Forbid("You don’t have access to this chat.");

                var messages = await _history.GetMessagesAsync(id, maxMessages: 50);

                var response = messages.Select(m => new {
                    sender = m.Role,
                    text = m.Message,
                    messageId = m.Id,
                    isHelpful = m.IsHelpful
                });

                return Ok(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"🔥 Failed to load messages: {ex.Message}");
                return StatusCode(500, new { error = "Unexpected server error." });
            }
        }


        // POST /api/chat/rate
        [HttpPost("rate")]
        public async Task<IActionResult> RateMessage([FromBody] RateRequest req)
        {
            var message = await _history.GetMessageByIdAsync(req.MessageId);
            if (message == null)
                return NotFound(new { error = "Message not found." });

            message.IsHelpful = req.IsHelpful;
            await _history.SaveAsync();

            return Ok(new { status = "Rated successfully." });
        }


    }
}
