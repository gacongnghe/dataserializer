using GameDataLibrary.Core.Services;

namespace GameDataLibrary.Core;

/// <summary>
/// Extension methods for the GameDataParser to provide convenient functionality
/// </summary>
public static class GameDataParserExtensions
{
    /// <summary>
    /// Parses all YAML files in the default schema/gamedata directory
    /// </summary>
    /// <param name="parser">The GameDataParser instance</param>
    /// <returns>Collection of ParsedGameData objects</returns>
    public static async Task<IEnumerable<Models.ParsedGameData>> ParseDefaultSchemaDirectoryAsync(this GameDataParser parser)
    {
        var defaultPath = Path.Combine(Directory.GetCurrentDirectory(), "schema", "gamedata");
        return await parser.ParseDirectoryAsync(defaultPath);
    }

    /// <summary>
    /// Validates all YAML files in the default schema/gamedata directory
    /// </summary>
    /// <param name="parser">The GameDataParser instance</param>
    /// <returns>True if all files parse successfully, false otherwise</returns>
    public static async Task<bool> ValidateDefaultSchemaDirectoryAsync(this GameDataParser parser)
    {
        var defaultPath = Path.Combine(Directory.GetCurrentDirectory(), "schema", "gamedata");
        return await parser.ValidateAllFilesAsync(defaultPath);
    }

    /// <summary>
    /// Gets a summary of all parsed schemas with their key information
    /// </summary>
    /// <param name="results">Collection of parsed game data results</param>
    /// <returns>Formatted string summary</returns>
    public static string GetSummary(this IEnumerable<Models.ParsedGameData> results)
    {
        var resultsList = results.ToList();
        var validCount = resultsList.Count(r => r.IsValid);
        var invalidCount = resultsList.Count(r => !r.IsValid);

        var summary = $"Parsed {resultsList.Count} YAML files:\n";
        summary += $"- Valid: {validCount}\n";
        summary += $"- Invalid: {invalidCount}\n\n";

        foreach (var result in resultsList)
        {
            var status = result.IsValid ? "✓" : "✗";
            summary += $"{status} {result.FileName} ({result.Schema.Name})\n";
            
            if (!result.IsValid && !string.IsNullOrEmpty(result.ErrorMessage))
            {
                summary += $"  Error: {result.ErrorMessage}\n";
            }
        }

        return summary;
    }
}