using GameDataLibrary.Core.Models;
using GameDataLibrary.Core.Services;
using Xunit;

namespace GameDataLibrary.Tests.Integration;

public class PlayerMarketNameSerializationTests
{
    private readonly GameDataSerializer _serializer;

    public PlayerMarketNameSerializationTests()
    {
        _serializer = new GameDataSerializer();
    }

    [Fact]
    public void Deserialize_PlayerMarketName_WithProvidedBytes_ReturnsCorrectObject()
    {
        // Arrange - The test bytes from the user's request
        // Bytes: 90 26 00 00 21 16 61 00 73 00 64 00 73 00 61 00 64 00 73 00 64 00 73 00 64 00 00 00
        var testBytes = new byte[] { 
            0x90, 0x26, 0x00, 0x00,  // roleId = 9872 (0x2690)
            0x21,                    // crc = 33 (0x21)
            0x16,                    // compact length = 22 (0x16)
            0x61, 0x00, 0x73, 0x00, 0x64, 0x00, 0x73, 0x00, 0x61, 0x00, 0x64, 0x00, 0x73, 0x00, 0x64, 0x00, 0x73, 0x00, 0x64, 0x00, 0x00, 0x00  // UTF-16 "asdsadsdsd" + null terminator
        };

        // Register the schema
        var playerMarketNameSchema = CreatePlayerMarketNameSchema();
        _serializer.RegisterSchema(playerMarketNameSchema);

        // Act
        var result = _serializer.Deserialize(testBytes, playerMarketNameSchema);

        // Assert
        Assert.NotNull(result);
        
        // Expected values from the user's description:
        // {roleId=9872, crc=33, name=asdsadsdsd}
        Assert.Equal(9872u, result.GetProperty<uint>("roleId"));
        Assert.Equal(33, result.GetProperty<byte>("crc"));
        Assert.Equal("asdsadsdsd", result.GetProperty<string>("name"));
    }

    [Fact]
    public void Serialize_PlayerMarketName_WithProvidedValues_ReturnsCorrectBytes()
    {
        // Arrange
        var playerMarketNameSchema = CreatePlayerMarketNameSchema();
        _serializer.RegisterSchema(playerMarketNameSchema);

        var playerMarketName = new DynamicGameDataObject(playerMarketNameSchema);
        playerMarketName.SetProperty("roleId", 9872u);
        playerMarketName.SetProperty("crc", (byte)33);
        playerMarketName.SetProperty("name", "asdsadsdsd");

        // Act
        var result = _serializer.Serialize(playerMarketName);

        // Assert - Expected bytes: 90 26 00 00 21 16 61 00 73 00 64 00 73 00 61 00 64 00 73 00 64 00 73 00 64 00 00 00
        var expectedBytes = new byte[] { 
            0x90, 0x26, 0x00, 0x00,  // roleId = 9872 (0x2690)
            0x21,                    // crc = 33 (0x21)
            0x16,                    // compact length = 22 (0x16)
            0x61, 0x00, 0x73, 0x00, 0x64, 0x00, 0x73, 0x00, 0x61, 0x00, 0x64, 0x00, 0x73, 0x00, 0x64, 0x00, 0x73, 0x00, 0x64, 0x00, 0x00, 0x00  // UTF-16 "asdsadsdsd" + null terminator
        };
        Assert.Equal(expectedBytes, result);
    }

    [Fact]
    public void SerializeDeserialize_PlayerMarketName_RoundTrip_ReturnsOriginalData()
    {
        // Arrange
        var playerMarketNameSchema = CreatePlayerMarketNameSchema();
        _serializer.RegisterSchema(playerMarketNameSchema);

        var originalPlayerMarketName = new DynamicGameDataObject(playerMarketNameSchema);
        originalPlayerMarketName.SetProperty("roleId", 12345u);
        originalPlayerMarketName.SetProperty("crc", (byte)100);
        originalPlayerMarketName.SetProperty("name", "TestPlayerName");

        // Act
        var serialized = _serializer.Serialize(originalPlayerMarketName);
        var deserialized = _serializer.Deserialize(serialized, playerMarketNameSchema);

        // Assert
        Assert.Equal(originalPlayerMarketName.GetProperty<uint>("roleId"), deserialized.GetProperty<uint>("roleId"));
        Assert.Equal(originalPlayerMarketName.GetProperty<byte>("crc"), deserialized.GetProperty<byte>("crc"));
        Assert.Equal(originalPlayerMarketName.GetProperty<string>("name"), deserialized.GetProperty<string>("name"));
    }

    [Fact]
    public void SerializeDeserialize_StringWithDifferentEncodings_HandlesCorrectly()
    {
        // Test ASCII encoding
        var asciiSchema = new GameDataSchema
        {
            Name = "StringTest",
            Type = "object",
            Properties = new Dictionary<string, PropertyDefinition>
            {
                ["text"] = new PropertyDefinition 
                { 
                    Type = "string",
                    Meta = new Dictionary<string, object>
                    {
                        ["encoding"] = "ascii",
                        ["size"] = 0
                    }
                }
            }
        };

        var asciiObject = new DynamicGameDataObject(asciiSchema);
        asciiObject.SetProperty("text", "Hello World");

        var asciiSerialized = _serializer.Serialize(asciiObject);
        var asciiDeserialized = _serializer.Deserialize(asciiSerialized, asciiSchema);
        Assert.Equal("Hello World", asciiDeserialized.GetProperty<string>("text"));

        // Test UTF8 encoding
        var utf8Schema = new GameDataSchema
        {
            Name = "StringTest",
            Type = "object",
            Properties = new Dictionary<string, PropertyDefinition>
            {
                ["text"] = new PropertyDefinition 
                { 
                    Type = "string",
                    Meta = new Dictionary<string, object>
                    {
                        ["encoding"] = "utf8",
                        ["size"] = 0
                    }
                }
            }
        };

        var utf8Object = new DynamicGameDataObject(utf8Schema);
        utf8Object.SetProperty("text", "Hello 世界");

        var utf8Serialized = _serializer.Serialize(utf8Object);
        var utf8Deserialized = _serializer.Deserialize(utf8Serialized, utf8Schema);
        Assert.Equal("Hello 世界", utf8Deserialized.GetProperty<string>("text"));

        // Test UTF16 encoding
        var utf16Schema = new GameDataSchema
        {
            Name = "StringTest",
            Type = "object",
            Properties = new Dictionary<string, PropertyDefinition>
            {
                ["text"] = new PropertyDefinition 
                { 
                    Type = "string",
                    Meta = new Dictionary<string, object>
                    {
                        ["encoding"] = "utf16",
                        ["size"] = 0
                    }
                }
            }
        };

        var utf16Object = new DynamicGameDataObject(utf16Schema);
        utf16Object.SetProperty("text", "Hello 世界");

        var utf16Serialized = _serializer.Serialize(utf16Object);
        var utf16Deserialized = _serializer.Deserialize(utf16Serialized, utf16Schema);
        Assert.Equal("Hello 世界", utf16Deserialized.GetProperty<string>("text"));
    }

    [Fact]
    public void SerializeDeserialize_StringWithDifferentSizes_HandlesCorrectly()
    {
        // Test size = 0 (auto-detect)
        var autoSizeSchema = new GameDataSchema
        {
            Name = "StringTest",
            Type = "object",
            Properties = new Dictionary<string, PropertyDefinition>
            {
                ["text"] = new PropertyDefinition 
                { 
                    Type = "string",
                    Meta = new Dictionary<string, object>
                    {
                        ["encoding"] = "utf8",
                        ["size"] = 0
                    }
                }
            }
        };

        var autoSizeObject = new DynamicGameDataObject(autoSizeSchema);
        autoSizeObject.SetProperty("text", "Test");

        var autoSizeSerialized = _serializer.Serialize(autoSizeObject);
        var autoSizeDeserialized = _serializer.Deserialize(autoSizeSerialized, autoSizeSchema);
        Assert.Equal("Test", autoSizeDeserialized.GetProperty<string>("text"));

        // Test size = 1 (byte length)
        var byteSizeSchema = new GameDataSchema
        {
            Name = "StringTest",
            Type = "object",
            Properties = new Dictionary<string, PropertyDefinition>
            {
                ["text"] = new PropertyDefinition 
                { 
                    Type = "string",
                    Meta = new Dictionary<string, object>
                    {
                        ["encoding"] = "utf8",
                        ["size"] = 1
                    }
                }
            }
        };

        var byteSizeObject = new DynamicGameDataObject(byteSizeSchema);
        byteSizeObject.SetProperty("text", "Hi");

        var byteSizeSerialized = _serializer.Serialize(byteSizeObject);
        var byteSizeDeserialized = _serializer.Deserialize(byteSizeSerialized, byteSizeSchema);
        Assert.Equal("Hi", byteSizeDeserialized.GetProperty<string>("text"));

        // Test size = 2 (ushort length)
        var ushortSizeSchema = new GameDataSchema
        {
            Name = "StringTest",
            Type = "object",
            Properties = new Dictionary<string, PropertyDefinition>
            {
                ["text"] = new PropertyDefinition 
                { 
                    Type = "string",
                    Meta = new Dictionary<string, object>
                    {
                        ["encoding"] = "utf8",
                        ["size"] = 2
                    }
                }
            }
        };

        var ushortSizeObject = new DynamicGameDataObject(ushortSizeSchema);
        ushortSizeObject.SetProperty("text", "Hello World");

        var ushortSizeSerialized = _serializer.Serialize(ushortSizeObject);
        var ushortSizeDeserialized = _serializer.Deserialize(ushortSizeSerialized, ushortSizeSchema);
        Assert.Equal("Hello World", ushortSizeDeserialized.GetProperty<string>("text"));

        // Test size = 4 (int length)
        var intSizeSchema = new GameDataSchema
        {
            Name = "StringTest",
            Type = "object",
            Properties = new Dictionary<string, PropertyDefinition>
            {
                ["text"] = new PropertyDefinition 
                { 
                    Type = "string",
                    Meta = new Dictionary<string, object>
                    {
                        ["encoding"] = "utf8",
                        ["size"] = 4
                    }
                }
            }
        };

        var intSizeObject = new DynamicGameDataObject(intSizeSchema);
        intSizeObject.SetProperty("text", "Hello World Test");

        var intSizeSerialized = _serializer.Serialize(intSizeObject);
        var intSizeDeserialized = _serializer.Deserialize(intSizeSerialized, intSizeSchema);
        Assert.Equal("Hello World Test", intSizeDeserialized.GetProperty<string>("text"));

        // Test size = -1 (compact integer)
        var compactSizeSchema = new GameDataSchema
        {
            Name = "StringTest",
            Type = "object",
            Properties = new Dictionary<string, PropertyDefinition>
            {
                ["text"] = new PropertyDefinition 
                { 
                    Type = "string",
                    Meta = new Dictionary<string, object>
                    {
                        ["encoding"] = "utf8",
                        ["size"] = -1
                    }
                }
            }
        };

        var compactSizeObject = new DynamicGameDataObject(compactSizeSchema);
        compactSizeObject.SetProperty("text", "Compact Test");

        var compactSizeSerialized = _serializer.Serialize(compactSizeObject);
        var compactSizeDeserialized = _serializer.Deserialize(compactSizeSerialized, compactSizeSchema);
        Assert.Equal("Compact Test", compactSizeDeserialized.GetProperty<string>("text"));
    }

    [Fact]
    public void CompactSint32_Serialization_HandlesCorrectly()
    {
        // Test simple case first
        var schema = new GameDataSchema
        {
            Name = "CompactTest",
            Type = "object",
            Properties = new Dictionary<string, PropertyDefinition>
            {
                ["text"] = new PropertyDefinition 
                { 
                    Type = "string",
                    Meta = new Dictionary<string, object>
                    {
                        ["encoding"] = "utf8",
                        ["size"] = -1
                    }
                }
            }
        };

        var testString = "Hello";
        var testObject = new DynamicGameDataObject(schema);
        testObject.SetProperty("text", testString);

        var serialized = _serializer.Serialize(testObject);
        var deserialized = _serializer.Deserialize(serialized, schema);
        
        Assert.Equal(testString, deserialized.GetProperty<string>("text"));
    }

    /// <summary>
    /// Creates the PlayerMarketName schema for testing
    /// </summary>
    private GameDataSchema CreatePlayerMarketNameSchema()
    {
        return new GameDataSchema
        {
            Name = "PlayerMarketName",
            Type = "object",
            Properties = new Dictionary<string, PropertyDefinition>
            {
                ["roleId"] = new PropertyDefinition { Type = "uint" },
                ["crc"] = new PropertyDefinition { Type = "byte" },
                ["name"] = new PropertyDefinition 
                { 
                    Type = "string",
                    Meta = new Dictionary<string, object>
                    {
                        ["encoding"] = "utf16",
                        ["size"] = -1
                    }
                }
            }
        };
    }
}