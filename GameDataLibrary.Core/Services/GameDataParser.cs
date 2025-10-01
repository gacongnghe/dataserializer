using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using GameDataLibrary.Core.Models;

namespace GameDataLibrary.Core.Services;

/// <summary>
/// Service for parsing YAML game data files
/// </summary>
public class GameDataParser
{
    private readonly IDeserializer _deserializer;

    public GameDataParser()
    {
        _deserializer = new DeserializerBuilder()
            .WithNamingConvention(UnderscoredNamingConvention.Instance)
            .Build();
    }

    /// <summary>
    /// Parses a single YAML file into a GameDataSchema
    /// </summary>
    /// <param name="filePath">Path to the YAML file</param>
    /// <returns>ParsedGameData containing the schema and metadata</returns>
    public async Task<ParsedGameData> ParseFileAsync(string filePath)
    {
        var result = new ParsedGameData
        {
            FileName = Path.GetFileName(filePath),
            FilePath = filePath
        };

        try
        {
            if (!File.Exists(filePath))
            {
                result.ErrorMessage = $"File not found: {filePath}";
                return result;
            }

            var yamlContent = await File.ReadAllTextAsync(filePath);
            var schema = _deserializer.Deserialize<GameDataSchema>(yamlContent);
            
            result.Schema = schema;
            result.IsValid = true;
        }
        catch (Exception ex)
        {
            result.ErrorMessage = ex.Message;
            result.IsValid = false;
        }

        return result;
    }

    /// <summary>
    /// Parses all YAML files in a directory
    /// </summary>
    /// <param name="directoryPath">Path to the directory containing YAML files</param>
    /// <returns>Collection of ParsedGameData objects</returns>
    public async Task<IEnumerable<ParsedGameData>> ParseDirectoryAsync(string directoryPath)
    {
        if (!Directory.Exists(directoryPath))
        {
            throw new DirectoryNotFoundException($"Directory not found: {directoryPath}");
        }

        var yamlFiles = Directory.GetFiles(directoryPath, "*.yml", SearchOption.TopDirectoryOnly);
        var results = new List<ParsedGameData>();

        foreach (var file in yamlFiles)
        {
            var result = await ParseFileAsync(file);
            results.Add(result);
        }

        return results;
    }

    /// <summary>
    /// Validates that all YAML files in a directory can be parsed successfully
    /// </summary>
    /// <param name="directoryPath">Path to the directory containing YAML files</param>
    /// <returns>True if all files parse successfully, false otherwise</returns>
    public async Task<bool> ValidateAllFilesAsync(string directoryPath)
    {
        var results = await ParseDirectoryAsync(directoryPath);
        return results.All(r => r.IsValid);
    }

    /// <summary>
    /// Gets validation summary for all files in a directory
    /// </summary>
    /// <param name="directoryPath">Path to the directory containing YAML files</param>
    /// <returns>Validation summary with counts and error details</returns>
    public async Task<ValidationSummary> GetValidationSummaryAsync(string directoryPath)
    {
        var results = await ParseDirectoryAsync(directoryPath);
        
        return new ValidationSummary
        {
            TotalFiles = results.Count(),
            ValidFiles = results.Count(r => r.IsValid),
            InvalidFiles = results.Count(r => !r.IsValid),
            Results = results.ToList()
        };
    }
}

/// <summary>
/// Summary of validation results for YAML files
/// </summary>
public class ValidationSummary
{
    public int TotalFiles { get; set; }
    public int ValidFiles { get; set; }
    public int InvalidFiles { get; set; }
    public List<ParsedGameData> Results { get; set; } = new();
    
    public bool AllValid => InvalidFiles == 0;
}