using GameDataLibrary.Core.Models;
using System.Text;

namespace GameDataLibrary.Core.Services;

/// <summary>
/// Service for serializing and deserializing game data objects based on schemas
/// </summary>
public class GameDataSerializer
{
    private readonly Dictionary<string, GameDataSchema> _schemaCache = new();

    /// <summary>
    /// Registers a schema for reference resolution
    /// </summary>
    /// <param name="schema">Schema to register</param>
    public void RegisterSchema(GameDataSchema schema)
    {
        if (!string.IsNullOrEmpty(schema.Name))
        {
            _schemaCache[schema.Name] = schema;
        }
    }

    /// <summary>
    /// Gets a registered schema by name
    /// </summary>
    /// <param name="schemaName">Name of the schema</param>
    /// <returns>Registered schema or null if not found</returns>
    public GameDataSchema? GetSchema(string schemaName)
    {
        _schemaCache.TryGetValue(schemaName, out var schema);
        return schema;
    }

    /// <summary>
    /// Gets a schema by reference name (e.g., "playerinfo.0x00.yml" -> "PlayerInfo")
    /// </summary>
    /// <param name="referenceName">Reference name from schema</param>
    /// <returns>Registered schema or null if not found</returns>
    private GameDataSchema? GetSchemaByReference(string referenceName)
    {
        // Try to find schema by matching the reference name pattern
        // For example: "playerinfo.0x00.yml" -> "PlayerInfo"
        var fileName = Path.GetFileNameWithoutExtension(referenceName);
        // Remove the version part (e.g., "playerinfo.0x00" -> "playerinfo")
        var schemaName = fileName.Split('.')[0];
        
        // Convert to PascalCase - handle camelCase to PascalCase
        // For "playerinfo" -> "PlayerInfo", we need to capitalize after "player"
        if (schemaName == "playerinfo")
        {
            schemaName = "PlayerInfo";
        }
        else
        {
            // General case: capitalize first letter
            schemaName = char.ToUpper(schemaName[0]) + schemaName.Substring(1);
        }
        
        
        return GetSchema(schemaName);
    }

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
                SerializeString(writer, propertyDefinition, value);
                break;

            case "point3f":
                SerializePoint3F(writer, value);
                break;

            case "timestamp":
                SerializeTimestamp(writer, value);
                break;

            case "array":
                SerializeArray(writer, propertyDefinition, value);
                break;

            default:
                // Check for reference types
                if (propertyDefinition.Type.StartsWith("ref("))
                {
                    SerializeReference(writer, propertyDefinition, value);
                }
                else
                {
                    throw new InvalidOperationException($"Unsupported property type: {propertyDefinition.Type}");
                }
                break;
        }
    }

    /// <summary>
    /// Serializes a Point3F object
    /// </summary>
    private void SerializePoint3F(BinaryWriter writer, object? value)
    {
        if (value is Point3F point3f)
        {
            writer.Write(point3f.X);
            writer.Write(point3f.Z);
            writer.Write(point3f.Y);
        }
        else if (value is Dictionary<string, object> dict)
        {
            // Handle dictionary representation
            var x = Convert.ToSingle(dict.GetValueOrDefault("x", 0f));
            var z = Convert.ToSingle(dict.GetValueOrDefault("z", 0f));
            var y = Convert.ToSingle(dict.GetValueOrDefault("y", 0f));
            writer.Write(x);
            writer.Write(z);
            writer.Write(y);
        }
        else
        {
            // Default to zero values
            writer.Write(0f);
            writer.Write(0f);
            writer.Write(0f);
        }
    }

    /// <summary>
    /// Serializes a timestamp (DateTime) as a 4-byte Unix timestamp
    /// </summary>
    private void SerializeTimestamp(BinaryWriter writer, object? value)
    {
        if (value is DateTime dateTime)
        {
            var timestamp = (uint)((DateTimeOffset)dateTime).ToUnixTimeSeconds();
            writer.Write(timestamp);
        }
        else if (value is string dateTimeString && DateTime.TryParse(dateTimeString, out var parsedDateTime))
        {
            var timestamp = (uint)((DateTimeOffset)parsedDateTime).ToUnixTimeSeconds();
            writer.Write(timestamp);
        }
        else
        {
            // Default to zero timestamp (Unix epoch)
            writer.Write(0u);
        }
    }

    /// <summary>
    /// Serializes a string with encoding and size handling
    /// </summary>
    private void SerializeString(BinaryWriter writer, PropertyDefinition propertyDefinition, object? value)
    {
        var stringValue = value?.ToString() ?? string.Empty;
        var encoding = propertyDefinition.GetStringEncoding() ?? "utf8";
        var size = propertyDefinition.GetStringSize() ?? 0;

        byte[] stringBytes;
        switch (encoding)
        {
            case "ascii":
                stringBytes = Encoding.ASCII.GetBytes(stringValue);
                break;
            case "utf8":
                stringBytes = Encoding.UTF8.GetBytes(stringValue);
                break;
            case "utf16":
                stringBytes = Encoding.Unicode.GetBytes(stringValue);
                // Add null terminator for UTF-16
                var nullTerminator = new byte[] { 0x00, 0x00 };
                stringBytes = stringBytes.Concat(nullTerminator).ToArray();
                break;
            default:
                throw new InvalidOperationException($"Unsupported string encoding: {encoding}");
        }

        if (size == 0)
        {
            // Auto-detect by null character - write length then data
            writer.Write(stringBytes.Length);
            writer.Write(stringBytes);
        }
        else if (size > 0)
        {
            // Fixed number of bytes for length
            if (size == 1)
            {
                writer.Write((byte)stringBytes.Length);
            }
            else if (size == 2)
            {
                writer.Write((ushort)stringBytes.Length);
            }
            else if (size == 4)
            {
                writer.Write(stringBytes.Length);
            }
            else
            {
                throw new InvalidOperationException($"Unsupported string size: {size}");
            }
            writer.Write(stringBytes);
        }
        else if (size == -1)
        {
            // Compact integer for length
            WriteCompactSint32(writer, stringBytes.Length);
            writer.Write(stringBytes);
        }
        else
        {
            throw new InvalidOperationException($"Invalid string size: {size}");
        }
    }

    /// <summary>
    /// Serializes an array
    /// </summary>
    private void SerializeArray(BinaryWriter writer, PropertyDefinition propertyDefinition, object? value)
    {
        var arraySize = propertyDefinition.GetArraySize();
        var itemType = propertyDefinition.GetArrayItemType();

        if (value is IEnumerable<object> array)
        {
            var arrayList = array.ToList();
            var count = arrayList.Count;

            // Write array length based on size meta
            if (arraySize == 1)
            {
                writer.Write((byte)count);
            }
            else if (arraySize == 2)
            {
                writer.Write((ushort)count);
            }
            else if (arraySize == 4)
            {
                writer.Write(count);
            }
            else
            {
                writer.Write(count); // Default to 4 bytes
            }

            // Serialize each item
            foreach (var item in arrayList)
            {
                if (itemType?.StartsWith("ref(") == true)
                {
                    // Handle reference items
                    var refSchemaName = itemType.Substring(4, itemType.Length - 5);
                    // Try to find schema by the reference name or by extracting the schema name
                    var refSchema = GetSchema(refSchemaName) ?? GetSchemaByReference(refSchemaName);
                    if (refSchema != null && item is DynamicGameDataObject refObject)
                    {
                        var itemBytes = Serialize(refObject);
                        writer.Write(itemBytes);
                    }
                }
                else
                {
                    // Handle primitive items
                    var itemDef = new PropertyDefinition { Type = itemType ?? "object" };
                    SerializeProperty(writer, itemDef, item);
                }
            }
        }
        else
        {
            // Write zero count
            if (arraySize == 1)
            {
                writer.Write((byte)0);
            }
            else if (arraySize == 2)
            {
                writer.Write((ushort)0);
            }
            else
            {
                writer.Write(0);
            }
        }
    }

    /// <summary>
    /// Serializes a reference type
    /// </summary>
    private void SerializeReference(BinaryWriter writer, PropertyDefinition propertyDefinition, object? value)
    {
        var refSchemaName = propertyDefinition.GetReferencedSchema();
        if (refSchemaName != null)
        {
            var refSchema = GetSchema(refSchemaName);
            if (refSchema != null && value is DynamicGameDataObject refObject)
            {
                var refBytes = Serialize(refObject);
                writer.Write(refBytes);
            }
            else
            {
                // Write empty bytes for the reference
                var expectedLength = GetExpectedByteLength(refSchema ?? new GameDataSchema());
                writer.Write(new byte[expectedLength]);
            }
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
            "string" => DeserializeString(reader, propertyDefinition),
            "point3f" => DeserializePoint3F(reader),
            "timestamp" => DeserializeTimestamp(reader),
            "array" => DeserializeArray(reader, propertyDefinition),
            _ => propertyDefinition.Type.StartsWith("ref(") 
                ? DeserializeReference(reader, propertyDefinition)
                : throw new InvalidOperationException($"Unsupported property type: {propertyDefinition.Type}")
        };
    }

    /// <summary>
    /// Deserializes a Point3F object
    /// </summary>
    private Point3F DeserializePoint3F(BinaryReader reader)
    {
        var x = reader.ReadSingle();
        var z = reader.ReadSingle();
        var y = reader.ReadSingle();
        return new Point3F(x, z, y);
    }

    /// <summary>
    /// Deserializes a timestamp (4-byte Unix timestamp) to DateTime
    /// </summary>
    private DateTime DeserializeTimestamp(BinaryReader reader)
    {
        var timestamp = reader.ReadUInt32();
        return DateTimeOffset.FromUnixTimeSeconds(timestamp).UtcDateTime;
    }

    /// <summary>
    /// Deserializes a string with encoding and size handling
    /// </summary>
    private string DeserializeString(BinaryReader reader, PropertyDefinition propertyDefinition)
    {
        var encoding = propertyDefinition.GetStringEncoding() ?? "utf8";
        var size = propertyDefinition.GetStringSize() ?? 0;

        int stringLength;
        if (size == 0)
        {
            // Auto-detect by null character - read length then data
            stringLength = reader.ReadInt32();
        }
        else if (size > 0)
        {
            // Fixed number of bytes for length
            if (size == 1)
            {
                stringLength = reader.ReadByte();
            }
            else if (size == 2)
            {
                stringLength = reader.ReadUInt16();
            }
            else if (size == 4)
            {
                stringLength = reader.ReadInt32();
            }
            else
            {
                throw new InvalidOperationException($"Unsupported string size: {size}");
            }
        }
        else if (size == -1)
        {
            // Compact integer for length
            stringLength = ReadCompactSint32(reader);
        }
        else
        {
            throw new InvalidOperationException($"Invalid string size: {size}");
        }

        var stringBytes = reader.ReadBytes(stringLength);
        
        switch (encoding)
        {
            case "ascii":
                return Encoding.ASCII.GetString(stringBytes);
            case "utf8":
                return Encoding.UTF8.GetString(stringBytes);
            case "utf16":
                // Remove null terminator if present
                if (stringBytes.Length >= 2 && stringBytes[stringBytes.Length - 2] == 0 && stringBytes[stringBytes.Length - 1] == 0)
                {
                    stringBytes = stringBytes.Take(stringBytes.Length - 2).ToArray();
                }
                return Encoding.Unicode.GetString(stringBytes);
            default:
                throw new InvalidOperationException($"Unsupported string encoding: {encoding}");
        }
    }

    /// <summary>
    /// Deserializes an array
    /// </summary>
    private List<object> DeserializeArray(BinaryReader reader, PropertyDefinition propertyDefinition)
    {
        var arraySize = propertyDefinition.GetArraySize();
        var itemType = propertyDefinition.GetArrayItemType();
        var result = new List<object>();

        // Read array length based on size meta
        int count;
        if (arraySize == 1)
        {
            count = reader.ReadByte();
        }
        else if (arraySize == 2)
        {
            count = reader.ReadUInt16();
        }
        else if (arraySize == 4)
        {
            count = reader.ReadInt32();
        }
        else
        {
            count = reader.ReadInt32(); // Default to 4 bytes
        }

        // Deserialize each item
        for (int i = 0; i < count; i++)
        {
            if (itemType?.StartsWith("ref(") == true)
            {
                // Handle reference items
                var refSchemaName = itemType.Substring(4, itemType.Length - 5);
                var refSchema = GetSchema(refSchemaName) ?? GetSchemaByReference(refSchemaName);
                if (refSchema != null)
                {
                    var expectedLength = GetExpectedByteLength(refSchema);
                    var itemBytes = reader.ReadBytes(expectedLength);
                    var refObject = Deserialize(itemBytes, refSchema);
                    result.Add(refObject);
                }
            }
            else
            {
                // Handle primitive items
                var itemDef = new PropertyDefinition { Type = itemType ?? "object" };
                var item = DeserializeProperty(reader, itemDef);
                result.Add(item);
            }
        }

        return result;
    }

    /// <summary>
    /// Deserializes a reference type
    /// </summary>
    private DynamicGameDataObject DeserializeReference(BinaryReader reader, PropertyDefinition propertyDefinition)
    {
        var refSchemaName = propertyDefinition.GetReferencedSchema();
        if (refSchemaName != null)
        {
            var refSchema = GetSchema(refSchemaName) ?? GetSchemaByReference(refSchemaName);
            if (refSchema != null)
            {
                var expectedLength = GetExpectedByteLength(refSchema);
                var refBytes = reader.ReadBytes(expectedLength);
                return Deserialize(refBytes, refSchema);
            }
        }
        
        // Return empty object if schema not found
        return new DynamicGameDataObject(new GameDataSchema());
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
            "int" or "int32" or "uint" or "uint32" or "float" or "timestamp" => 4,
            "long" or "int64" or "ulong" or "uint64" or "double" => 8,
            "short" or "int16" or "ushort" or "uint16" => 2,
            "byte" or "sbyte" or "bool" => 1,
            "string" => -1, // Variable length
            "point3f" => 12, // 3 floats = 12 bytes
            "array" => -1, // Variable length
            _ => typeName.StartsWith("ref(") ? -1 : throw new InvalidOperationException($"Unknown type: {typeName}")
        };
    }

    /// <summary>
    /// Writes a compact signed 32-bit integer to the stream
    /// Based on compact_sint32 from gnmarshal.h
    /// </summary>
    /// <param name="writer">Binary writer</param>
    /// <param name="value">Value to write</param>
    private void WriteCompactSint32(BinaryWriter writer, int value)
    {
        if (value >= 0)
        {
            if (value < 0x40)
            {
                writer.Write((byte)value);
            }
            else if (value < 0x2000)
            {
                writer.Write((ushort)(value | 0x8000));
            }
            else if (value < 0x10000000)
            {
                writer.Write((uint)(value | 0xc0000000));
            }
            else
            {
                writer.Write((byte)0xe0);
                writer.Write((uint)value);
            }
        }
        else
        {
            int absValue = -value;
            if (absValue > 0)
            {
                if (absValue < 0x40)
                {
                    writer.Write((byte)(absValue | 0x40));
                }
                else if (absValue < 0x2000)
                {
                    writer.Write((ushort)(absValue | 0xa000));
                }
                else if (absValue < 0x10000000)
                {
                    writer.Write((uint)(absValue | 0xd0000000));
                }
                else
                {
                    writer.Write((byte)0xf0);
                    writer.Write((uint)absValue);
                }
            }
            else
            {
                writer.Write((byte)0xf0);
                writer.Write((uint)value);
            }
        }
    }

    /// <summary>
    /// Reads a compact signed 32-bit integer from the stream
    /// Based on uncompact_sint32 from gnmarshal.h
    /// </summary>
    /// <param name="reader">Binary reader</param>
    /// <returns>Read value</returns>
    private int ReadCompactSint32(BinaryReader reader)
    {
        if (reader.BaseStream.Position >= reader.BaseStream.Length)
            throw new InvalidOperationException("End of stream reached");

        byte firstByte = reader.ReadByte();
        
        switch (firstByte & 0xf0)
        {
            case 0xf0:
                return -(int)reader.ReadUInt32();
            case 0xe0:
                return reader.ReadInt32();
            case 0xd0:
                return -(int)(reader.ReadUInt32() & ~0xd0000000);
            case 0xc0:
                return (int)(reader.ReadUInt32() & ~0xc0000000);
            case 0xb0:
            case 0xa0:
                return -(reader.ReadUInt16() & ~0xa000);
            case 0x90:
            case 0x80:
                return reader.ReadUInt16() & ~0x8000;
            case 0x70:
            case 0x60:
            case 0x50:
            case 0x40:
                return -(firstByte & ~0x40);
            default:
                return firstByte;
        }
    }
}