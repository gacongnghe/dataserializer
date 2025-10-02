using System.Collections.Concurrent;

namespace GameDataLibrary.Core.Models;

/// <summary>
/// Represents a dynamically created object based on a GameDataSchema
/// </summary>
public class DynamicGameDataObject
{
    private readonly Dictionary<string, object> _properties = new();
    private readonly GameDataSchema _schema;

    public DynamicGameDataObject(GameDataSchema schema)
    {
        _schema = schema ?? throw new ArgumentNullException(nameof(schema));
    }

    /// <summary>
    /// Gets the schema used to create this object
    /// </summary>
    public GameDataSchema Schema => _schema;

    /// <summary>
    /// Gets or sets a property value by name
    /// </summary>
    /// <param name="propertyName">Name of the property</param>
    /// <returns>Property value</returns>
    public object? this[string propertyName]
    {
        get => _properties.TryGetValue(propertyName, out var value) ? value : null;
        set
        {
            if (value != null)
            {
                _properties[propertyName] = value;
            }
            else
            {
                _properties.Remove(propertyName);
            }
        }
    }

    /// <summary>
    /// Gets all property names
    /// </summary>
    public IEnumerable<string> PropertyNames => _properties.Keys;

    /// <summary>
    /// Gets all property values
    /// </summary>
    public IEnumerable<KeyValuePair<string, object>> Properties => _properties;

    /// <summary>
    /// Gets a property value with type conversion
    /// </summary>
    /// <typeparam name="T">Expected type</typeparam>
    /// <param name="propertyName">Property name</param>
    /// <returns>Converted value</returns>
    public T? GetProperty<T>(string propertyName)
    {
        if (!_properties.TryGetValue(propertyName, out var value))
        {
            return default(T);
        }

        if (value is T directValue)
        {
            return directValue;
        }

        try
        {
            return (T)Convert.ChangeType(value, typeof(T));
        }
        catch
        {
            return default(T);
        }
    }

    /// <summary>
    /// Sets a property value
    /// </summary>
    /// <param name="propertyName">Property name</param>
    /// <param name="value">Value to set</param>
    public void SetProperty(string propertyName, object value)
    {
        _properties[propertyName] = value;
    }

    /// <summary>
    /// Checks if a property exists
    /// </summary>
    /// <param name="propertyName">Property name</param>
    /// <returns>True if property exists</returns>
    public bool HasProperty(string propertyName)
    {
        return _properties.ContainsKey(propertyName);
    }

    /// <summary>
    /// Gets the count of properties
    /// </summary>
    public int PropertyCount => _properties.Count;

    /// <summary>
    /// Creates a dictionary representation of the object
    /// </summary>
    /// <returns>Dictionary with property names and values</returns>
    public Dictionary<string, object> ToDictionary()
    {
        return new Dictionary<string, object>(_properties);
    }

    /// <summary>
    /// Creates a string representation of the object
    /// </summary>
    /// <returns>String representation</returns>
    public override string ToString()
    {
        var properties = string.Join(", ", _properties.Select(kvp => $"{kvp.Key}={kvp.Value}"));
        return $"{_schema.Name}({properties})";
    }
}