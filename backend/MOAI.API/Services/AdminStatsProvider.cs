namespace MOAI.API.Services;

public static class AdminStatsProvider
{
    public static object GetSnapshot()
    {
        return new
        {
            serverTime = DateTime.UtcNow,
            environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown",
            memoryUsageMB = Math.Round(GC.GetTotalMemory(false) / 1024.0 / 1024.0, 2),
            machineName = Environment.MachineName,
            uptimeMinutes = Math.Round(TimeSpan.FromMilliseconds(Environment.TickCount64).TotalMinutes, 1),
            activeUsers = ActiveUserTracker.GetCurrentSessionSummary()
        };
    }
}
