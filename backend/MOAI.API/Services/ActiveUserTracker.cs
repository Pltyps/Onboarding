namespace MOAI.API.Services;

public static class ActiveUserTracker
{
    private static readonly Dictionary<string, DateTime> _active = new();

    public static void Track(string email)
    {
        if (!string.IsNullOrEmpty(email))
        {
            _active[email] = DateTime.UtcNow;
        }
    }

    public static Dictionary<string, int> GetCurrentSessionSummary()
    {
        var cutoff = DateTime.UtcNow.AddMinutes(-15); // "active" if seen in last 15 min
        return _active
            .Where(kvp => kvp.Value > cutoff)
            .GroupBy(kvp => GetRoleFromEmail(kvp.Key))
            .ToDictionary(g => g.Key, g => g.Count());
    }

    private static string GetRoleFromEmail(string email)
    {
        if (email.Contains("admin")) return "Admin";
        if (email.Contains("employee")) return "FullTime";
        if (email.Contains("student")) return "Student";
        return "Unknown";
    }
}
