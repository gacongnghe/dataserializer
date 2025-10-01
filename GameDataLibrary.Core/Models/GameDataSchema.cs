using YamlDotNet.Serialization;

namespace GameDataLibrary.Core.Models;

/// <summary>
/// Represents a game data schema definition from YAML files
/// </summary>
public class GameDataSchema
{
    [YamlMember(Alias = "type")]
    public string Type { get; set; } = string.Empty;

    [YamlMember(Alias = "name")]
    public string Name { get; set; } = string.Empty;

    [YamlMember(Alias = "properties")]
    public Dictionary<string, PropertyDefinition> Properties { get; set; } = new();
}

/// <summary>
/// Represents a property definition within a schema
/// </summary>
public class PropertyDefinition
{
    [YamlMember(Alias = "type")]
    public string Type { get; set; } = string.Empty;

    [YamlMember(Alias = "meta")]
    public Dictionary<string, object>? Meta { get; set; }
}

/// <summary>
/// Represents parsed game data from a YAML file
/// </summary>
public class ParsedGameData
{
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public GameDataSchema Schema { get; set; } = new();
    public bool IsValid { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime ParsedAt { get; set; } = DateTime.UtcNow;
}