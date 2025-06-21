using System.ComponentModel.DataAnnotations;

namespace MOAI.API.Models
{
    public class ChatRequest
    {
        [Required]
        public required string Message { get; set; }
    }
}
