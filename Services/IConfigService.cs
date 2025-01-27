// Services/IConfigService.cs

using System.Collections.Generic;
using System.Threading.Tasks;
using CKPEConfig.Models;

namespace CKPEConfig.Services;

public interface IConfigService
{
    Task<(List<ConfigSection> Sections, List<string> Lines)> ParseIniWithCommentsAsync(string filePath);
    Task SaveIniAsync(string filePath, List<ConfigSection> sections, List<string> originalLines);
    string ParseComments(List<string> lines, int startIdx);
}