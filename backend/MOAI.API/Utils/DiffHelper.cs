namespace MOAI.API.Utils
{
    public static class DiffHelper
    {
        public static List<string> GenerateLineDiff(string oldContent, string newContent)
        {
            var oldLines = oldContent.Split('\n');
            var newLines = newContent.Split('\n');
            var diff = new List<string>();

            var max = Math.Max(oldLines.Length, newLines.Length);

            for (int i = 0; i < max; i++)
            {
                var oldLine = i < oldLines.Length ? oldLines[i].TrimEnd() : null;
                var newLine = i < newLines.Length ? newLines[i].TrimEnd() : null;

                if (oldLine == newLine)
                    diff.Add("  " + oldLine);
                else
                {
                    if (oldLine != null) diff.Add("- " + oldLine);
                    if (newLine != null) diff.Add("+ " + newLine);
                }
            }

            return diff;
        }
    }
}

