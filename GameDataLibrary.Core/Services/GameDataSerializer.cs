using GameDataLibrary.Core.Models;
using System.Text;

namespace GameDataLibrary.Core.Services;

/// <summary>
/// Service for serializing and deserializing game data objects based on schemas
/// </summary>
public class GameDataSerializer
{
    /// <summary>
    /// Serializes a DynamicGameDataObject to a byte array based on its schema
    /// </summary>
    /// <param name="gameDataObject">The object to serialize</param>
    /// <returns>Byte array representation</returns>
    /// <exception cref="ArgumentNullException">Thrown when gameDataObject is null</exception>
    /// <exception cref="InvalidOperationException">Thrown when schema is invalid or unsupported type</exception>
    public byte[] Serialize(DynamicGameDataObject gameDataObject)
    {
        if (gameDataObject == null)
            throw new ArgumentNullException(nameof(gameDataObject));

        var schema = gameDataObject.Schema;
        if (schema?.Properties == null)
            throw new InvalidOperationException("Invalid schema: Properties collection is null");

        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream);

        // Serialize properties in the order they appear in the schema
        foreach (var property in schema.Properties)
        {
            var propertyName = property.Key;
            var propertyDefinition = property.Value;
            var value = gameDataObject[propertyName];

            SerializeProperty(writer, propertyDefinition, value);
        }

        return stream.ToArray();
    }

    /// <summary>
    /// Deserializes a byte array to a DynamicGameDataObject based on a schema
    /// </summary>
    /// <param name="data">Byte array to deserialize</param>
    /// <param name="schema">Schema to use for deserialization</param>
    /// <returns>DynamicGameDataObject with deserialized data</returns>
    /// <exception cref="ArgumentNullException">Thrown when data or schema is null</exception>
    /// <exception cref="InvalidOperationException">Thrown when schema is invalid or unsupported type</exception>
    public DynamicGameDataObject Deserialize(byte[] data, GameDataSchema schema)
    {
        if (data == null)
            throw new ArgumentNullException(nameof(data));
        if (schema == null)
            throw new ArgumentNullException(nameof(schema));
        if (schema.Properties == null)
            throw new InvalidOperationException("Invalid schema: Properties collection is null");

        var gameDataObject = new DynamicGameDataObject(schema);

        using var stream = new MemoryStream(data);
        using var reader = new BinaryReader(stream);

        // Deserialize properties in the order they appear in the schema
        foreach (var property in schema.Properties)
        {
            var propertyName = property.Key;
            var propertyDefinition = property.Value;

            var value = DeserializeProperty(reader, propertyDefinition);
            gameDataObject.SetProperty(propertyName, value);
        }

        return gameDataObject;
    }

    /// <summary>
    /// Serializes a single property based on its definition
    /// </summary>
    /// <param name="writer">Binary writer</param>
    /// <param name="propertyDefinition">Property definition</param>
    /// <param name="value">Value to serialize</param>
    private void SerializeProperty(BinaryWriter writer, PropertyDefinition propertyDefinition, object? value)
    {
        var propertyType = propertyDefinition.Type.ToLowerInvariant();

        switch (propertyType)
        {
            case "int":
            case "int32":
                var intValue = value != null ? Convert.ToInt32(value) : 0;
                writer.Write(intValue);
                break;

            case "uint":
            case "uint32":
                var uintValue = value != null ? Convert.ToUInt32(value) : 0u;
                writer.Write(uintValue);
                break;

            case "long":
            case "int64":
                var longValue = value != null ? Convert.ToInt64(value) : 0L;
                writer.Write(longValue);
                break;

            case "ulong":
            case "uint64":
                var ulongValue = value != null ? Convert.ToUInt64(value) : 0UL;
                writer.Write(ulongValue);
                break;

            case "short":
            case "int16":
                var shortValue = value != null ? Convert.ToInt16(value) : (short)0;
                writer.Write(shortValue);
                break;

            case "ushort":
            case "uint16":
                var ushortValue = value != null ? Convert.ToUInt16(value) : (ushort)0;
                writer.Write(ushortValue);
                break;

            case "byte":
                var byteValue = value != null ? Convert.ToByte(value) : (byte)0;
                writer.Write(byteValue);
                break;

            case "sbyte":
                var sbyteValue = value != null ? Convert.ToSByte(value) : (sbyte)0;
                writer.Write(sbyteValue);
                break;

            case "float":
                var floatValue = value != null ? Convert.ToSingle(value) : 0f;
                writer.Write(floatValue);
                break;

            case "double":
                var doubleValue = value != null ? Convert.ToDouble(value) : 0.0;
                writer.Write(doubleValue);
                break;

            case "bool":
                var boolValue = value != null ? Convert.ToBoolean(value) : false;
                writer.Write(boolValue);
                break;

            case "string":
                var stringValue = value?.ToString() ?? string.Empty;
                var stringBytes = Encoding.UTF8.GetBytes(stringValue);
                writer.Write(stringBytes.Length);
                writer.Write(stringBytes);
                break;

            default:
                throw new InvalidOperationException($"Unsupported property type: {propertyDefinition.Type}");
        }
    }

    /// <summary>
    /// Deserializes a single property based on its definition
    /// </summary>
    /// <param name="reader">Binary reader</param>
    /// <param name="propertyDefinition">Property definition</param>
    /// <returns>Deserialized value</returns>
    private object DeserializeProperty(BinaryReader reader, PropertyDefinition propertyDefinition)
    {
        var propertyType = propertyDefinition.Type.ToLowerInvariant();

        return propertyType switch
        {
            "int" or "int32" => reader.ReadInt32(),
            "uint" or "uint32" => reader.ReadUInt32(),
            "long" or "int64" => reader.ReadInt64(),
            "ulong" or "uint64" => reader.ReadUInt64(),
            "short" or "int16" => reader.ReadInt16(),
            "ushort" or "uint16" => reader.ReadUInt16(),
            "byte" => reader.ReadByte(),
            "sbyte" => reader.ReadSByte(),
            "float" => reader.ReadSingle(),
            "double" => reader.ReadDouble(),
            "bool" => reader.ReadBoolean(),
            "string" => Encoding.UTF8.GetString(reader.ReadBytes(reader.ReadInt32())),
            _ => throw new InvalidOperationException($"Unsupported property type: {propertyDefinition.Type}")
        };
    }

    /// <summary>
    /// Gets the expected byte length for a schema
    /// </summary>
    /// <param name="schema">Schema to analyze</param>
    /// <returns>Expected byte length</returns>
    public int GetExpectedByteLength(GameDataSchema schema)
    {
        if (schema?.Properties == null)
            return 0;

        int totalLength = 0;
        foreach (var property in schema.Properties)
        {
            totalLength += GetTypeByteLength(property.Value.Type);
        }
        return totalLength;
    }

    /// <summary>
    /// Gets the byte length for a specific type
    /// </summary>
    /// <param name="typeName">Type name</param>
    /// <returns>Byte length</returns>
    private int GetTypeByteLength(string typeName)
    {
        return typeName.ToLowerInvariant() switch
        {
            "int" or "int32" or "uint" or "uint32" or "float" => 4,
            "long" or "int64" or "ulong" or "uint64" or "double" => 8,
            "short" or "int16" or "ushort" or "uint16" => 2,
            "byte" or "sbyte" or "bool" => 1,
            "string" => -1, // Variable length
            _ => throw new InvalidOperationException($"Unknown type: {typeName}")
        };
    }
}