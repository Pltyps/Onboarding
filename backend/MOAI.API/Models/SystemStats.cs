namespace MOAI.API.Models
{
    public class SystemStats
    {
        public int Id { get; set; }
        public int ActiveUserCount { get; set; }
        public int TotalRequests { get; set; }
        public int UptimeMinutes { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
