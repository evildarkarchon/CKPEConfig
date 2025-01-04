using System.IO;
namespace CKPEConfig;

public static class ConfigParser
{
    public static (List<ConfigSection> Sections, string[] Lines) ParseIniWithComments(string filePath)
    {
        var lines = File.ReadAllLines(filePath);
        var sections = new List<ConfigSection>();
        ConfigSection? currentSection = null;

        for (var i = 0; i < lines.Length; i++)
        {
            var line = lines[i].Trim();
            if (string.IsNullOrEmpty(line) || line.StartsWith(";"))
            {
                continue;
            }

            if (line.StartsWith("[") && line.EndsWith("]"))
            {
                var sectionName = line[1..^1];
                var tooltip = ParseComments(lines, i);
                currentSection = new ConfigSection(sectionName, tooltip, i);
                sections.Add(currentSection);
            }
            else if (line.Contains('=') && currentSection != null)
            {
                var parts = line.Split('=', 2);
                var name = parts[0].Trim();
                var value = parts[1].Trim();

                var tooltip = ParseComments(lines, i);
                string? inlineComment = null;

                if (value.Contains(';'))
                {
                    var commentParts = value.Split(';', 2);
                    value = commentParts[0].Trim();
                    inlineComment = commentParts[1].Trim();

                    if (!string.IsNullOrEmpty(tooltip))
                    {
                        tooltip += "\n" + inlineComment;
                    }
                    else
                    {
                        tooltip = inlineComment;
                    }
                }

                var entry = new ConfigEntry(name, value, tooltip, i)
                {
                    InlineComment = inlineComment
                };
                currentSection.Entries.Add(entry);
            }
        }

        return (sections, lines);
    }

    private static string? ParseComments(string[] lines, int startIdx)
    {
        var comments = new List<string>();
        var idx = startIdx - 1;

        while (idx >= 0 && (lines[idx].Trim().StartsWith(";") || string.IsNullOrWhiteSpace(lines[idx])))
        {
            if (lines[idx].Trim().StartsWith(";"))
            {
                comments.Insert(0, lines[idx].Trim()[1..].Trim());
            }
            idx--;
        }

        return comments.Count > 0 ? string.Join("\n", comments) : null;
    }
}