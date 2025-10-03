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

    /// <summary>
    /// Gets the referenced schema file name for ref types
    /// </summary>
    public string? GetReferencedSchema()
    {
        if (Type.StartsWith("ref(") && Type.EndsWith(")"))
        {
            return Type.Substring(4, Type.Length - 5);
        }
        return null;
    }

    /// <summary>
    /// Gets the array size from meta information
    /// </summary>
    public int? GetArraySize()
    {
        if (Meta?.TryGetValue("size", out var sizeObj) == true && sizeObj is int size)
        {
            return size;
        }
        return null;
    }

    /// <summary>
    /// Gets the item type for arrays from meta information
    /// </summary>
    public string? GetArrayItemType()
    {
        if (Meta?.TryGetValue("itemType", out var itemTypeObj) == true)
        {
            return itemTypeObj?.ToString();
        }
        return null;
    }

    /// <summary>
    /// Gets the encoding for string types from meta information
    /// </summary>
    public string? GetStringEncoding()
    {
        if (Meta?.TryGetValue("encoding", out var encodingObj) == true)
        {
            return encodingObj?.ToString()?.ToLowerInvariant();
        }
        return "utf8"; // Default to UTF8
    }

    /// <summary>
    /// Gets the size handling for string types from meta information
    /// </summary>
    public int? GetStringSize()
    {
        if (Meta?.TryGetValue("size", out var sizeObj) == true && sizeObj is int size)
        {
            return size;
        }
        return 0; // Default to auto-detect (null-terminated)
    }
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