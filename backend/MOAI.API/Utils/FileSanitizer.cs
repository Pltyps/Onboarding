namespace MOAI.API.Utils
{
    public static class FileSanitizer
    {
        private static readonly string[] DangerousPatterns = new[]
        {
            "<script", "</script", "SELECT * FROM", "DROP TABLE", "eval(", "onload=", "onclick=",
            "<?php", "require(", "include(", "system(", "exec(", "<iframe", "<object", "<embed"
        };

        public static bool IsContentSafe(string content)
        {
            var lower = content.ToLowerInvariant();

            return !DangerousPatterns.Any(p => lower.Contains(p));
        }

        public static string Sanitize(string content)
        {
            // Optional: strip certain patterns or normalize whitespace
            var lines = content.Split('\n')
                .Where(line => !line.ToLowerInvariant().Contains("<script"))
                .ToList();

            return string.Join('\n', lines);
        }
    }
}
