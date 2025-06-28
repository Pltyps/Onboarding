using OpenAI.Chat;

namespace MOAI.API.Models
{
    public class ChatSession
    {
        public int Id { get; set; }
        public string UserEmail { get; set; } = "";
        public string Title { get; set; } = ""; // Optional: user can name the chat
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public List<ChatMessage> Messages { get; set; } = new();
    }
}
