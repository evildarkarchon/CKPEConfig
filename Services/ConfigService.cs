// Services/ConfigService.cs

using System.Collections.Generic;
using System.IO;
using System.Linq;
using CKPEConfig.Models;
using System.Threading.Tasks;

namespace CKPEConfig.Services;

public class ConfigService : IConfigService
{
    public async Task<(List<ConfigSection> Sections, List<string> Lines)> ParseIniWithCommentsAsync(string filePath)
    {
        var lines = await File.ReadAllLinesAsync(filePath);
        var sections = new List<ConfigSection>();
        ConfigSection? currentSection = null;

        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i].Trim();
            if (string.IsNullOrEmpty(line) || line.StartsWith(";"))
                continue;

            if (line.StartsWith("[") && line.EndsWith("]"))
            {
                var sectionName = line.Substring(1, line.Length - 2);
                var tooltip = ParseComments(lines.ToList(), i);
                currentSection = new ConfigSection(sectionName, tooltip, i);
                sections.Add(currentSection);
            }
            else if (line.Contains('=') && currentSection != null)
            {
                var parts = line.Split(new[] { '=' }, 2);
                var name = parts[0].Trim();
                var value = parts[1].Trim();

                var tooltip = ParseComments(lines.ToList(), i);
                var inlineComment = string.Empty;

                if (value.Contains(';'))
                {
                    var commentParts = value.Split(new[] { ';' }, 2);
                    value = commentParts[0].Trim();
                    inlineComment = commentParts[1].Trim();
                    
                    if (!string.IsNullOrEmpty(tooltip))
                        tooltip += "\n" + inlineComment;
                    else
                        tooltip = inlineComment;
                }

                var entry = new ConfigEntry(name, value, tooltip, i)
                {
                    InlineComment = inlineComment
                };
                currentSection.Entries.Add(entry);
            }
        }

        return (sections, lines.ToList());
    }

    public string ParseComments(List<string> lines, int startIdx)
    {
        var comments = new List<string>();
        var idx = startIdx - 1;

        while (idx >= 0 && (lines[idx].Trim().StartsWith(";") || string.IsNullOrWhiteSpace(lines[idx])))
        {
            if (lines[idx].Trim().StartsWith(";"))
            {
                comments.Insert(0, lines[idx].Trim().Substring(1).Trim());
            }
            idx--;
        }

        return string.Join("\n", comments);
    }

    public async Task SaveIniAsync(string filePath, List<ConfigSection> sections, List<string> originalLines)
    {
        var newLines = new List<string>(originalLines);

        foreach (var section in sections)
        {
            foreach (var entry in section.Entries)
            {
                string newLine;
                if (!string.IsNullOrEmpty(entry.InlineComment))
                {
                    newLine = $"{entry.Name}={entry.Value}\t\t\t; {entry.InlineComment}";
                }
                else
                {
                    newLine = $"{entry.Name}={entry.Value}";
                }

                if (entry.LineNumber.HasValue)
                {
                    var leadingSpaces = originalLines[entry.LineNumber.Value].Length - 
                                      originalLines[entry.LineNumber.Value].TrimStart().Length;
                    newLines[entry.LineNumber.Value] = new string(' ', leadingSpaces) + newLine + "\n";
                }
                else
                {
                    newLines.Add(newLine + "\n");
                }
            }
        }

        await File.WriteAllLinesAsync(filePath, newLines);
    }
}