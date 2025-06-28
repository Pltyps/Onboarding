using System.ComponentModel.DataAnnotations;

namespace MOAI.API.Models
{
    public class ChatRequest
    {
        public string Message { get; set; } = "";
        public int ChatSessionId { get; set; }
        public bool IsFirstMessage { get; set; } = false;
    }

}
