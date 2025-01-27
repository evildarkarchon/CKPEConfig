// Services/ConfigService.cs

using System.Collections.Generic;
using System.IO;
using System.Linq;
using CKPEConfig.Models;
using System.Threading.Tasks;

namespace CKPEConfig.Services;

public class ConfigService : IConfigService
{
    /// <summary>
    /// Parses an INI configuration file, extracting sections, entries, and their associated comments.
    /// Supports inline comments and preceding line comments for both sections and entries.
    /// </summary>
    /// <param name="filePath">The path to the INI file to be parsed.</param>
    /// <returns>A tuple containing a list of parsed <see cref="ConfigSection"/> objects and the original lines of the INI file as a list of strings.</returns>
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
                var parts = line.Split(['='], 2);
                var name = parts[0].Trim();
                var value = parts[1].Trim();

                var tooltip = ParseComments(lines.ToList(), i);
                var inlineComment = string.Empty;

                if (value.Contains(';'))
                {
                    var commentParts = value.Split([';'], 2);
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

    /// <summary>
    /// Extracts comment lines preceding a specified index in a collection of lines, typically representing an INI file.
    /// Combines the extracted comments into a formatted string.
    /// </summary>
    /// <param name="lines">The list of lines from which comments are to be parsed.</param>
    /// <param name="startIdx">The index indicating where parsing should stop, examining lines before this index.</param>
    /// <returns>A string containing the concatenated comments preceding the specified index, separated by newlines.</returns>
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

    /// <summary>
    /// Writes the provided configuration sections and original INI file lines to a file.
    /// Updates or adds entries in the file based on the provided sections while preserving unmodified content and comments.
    /// </summary>
    /// <param name="filePath">The file path of the INI file to save the changes to.</param>
    /// <param name="sections">A list of configuration sections containing the entries to be saved or updated.</param>
    /// <param name="originalLines">The original lines of the INI file to retain unchanged content and layout.</param>
    /// <returns>A task that represents the asynchronous save operation.</returns>
    public async Task SaveIniAsync(string filePath, List<ConfigSection> sections, List<string> originalLines)
    {
        var newLines = new List<string>(originalLines);

        foreach (var entry in sections.SelectMany(section => section.Entries))
        {
            var newLine = !string.IsNullOrEmpty(entry.InlineComment) ? $"{entry.Name}={entry.Value}\t\t\t; {entry.InlineComment}" : $"{entry.Name}={entry.Value}";

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

        await File.WriteAllLinesAsync(filePath, newLines);
    }
}