using System.Text.Json.Serialization;

namespace MOAI.API.Models
{
    public class ChatMessage
    {
        public int Id { get; set; }
        public int ChatSessionId { get; set; }

        public string Role { get; set; } = ""; // "user" or "assistant"
        public string Message { get; set; } = "";
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public bool? IsHelpful { get; set; } // null = not rated, true = 👍, false = 👎

        [JsonIgnore] // 🛑 prevents circular reference
        public ChatSession ChatSession { get; set; } = null!;
    }
}
