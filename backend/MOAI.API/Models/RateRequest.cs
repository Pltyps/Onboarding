namespace MOAI.API.Models
{
    public class RateRequest
    {
        public int MessageId { get; set; }
        public bool IsHelpful { get; set; }
    }
}
