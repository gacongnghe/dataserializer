using GameDataLibrary.Core.Models;
using GameDataLibrary.Core.Services;
using Xunit;

namespace GameDataLibrary.Tests.Services;

public class GameDataSerializerTests
{
    private readonly GameDataSerializer _serializer;
    private readonly GameDataParser _parser;

    public GameDataSerializerTests()
    {
        _serializer = new GameDataSerializer();
        _parser = new GameDataParser();
    }

    [Fact]
    public void Serialize_WithValidObject_ReturnsByteArray()
    {
        // Arrange
        var schema = CreateElfVigorSchema();
        var gameDataObject = new DynamicGameDataObject(schema);
        gameDataObject.SetProperty("vigor", 1);
        gameDataObject.SetProperty("maxVigor", 64);
        gameDataObject.SetProperty("vigorGen", 128);

        // Act
        var result = _serializer.Serialize(gameDataObject);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(12, result.Length); // 3 int32 values = 12 bytes
    }

    [Fact]
    public void Deserialize_WithValidByteArray_ReturnsCorrectObject()
    {
        // Arrange
        var schema = CreateElfVigorSchema();
        var expectedBytes = new byte[] { 0x01, 0x00, 0x00, 0x00, 0x40, 0x00, 0x00, 0x00, 0x80, 0x00, 0x00, 0x00 };

        // Act
        var result = _serializer.Deserialize(expectedBytes, schema);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.GetProperty<int>("vigor"));
        Assert.Equal(64, result.GetProperty<int>("maxVigor"));
        Assert.Equal(128, result.GetProperty<int>("vigorGen"));
    }

    [Fact]
    public void SerializeDeserialize_RoundTrip_ReturnsOriginalData()
    {
        // Arrange
        var schema = CreateElfVigorSchema();
        var originalObject = new DynamicGameDataObject(schema);
        originalObject.SetProperty("vigor", 42);
        originalObject.SetProperty("maxVigor", 100);
        originalObject.SetProperty("vigorGen", 200);

        // Act
        var serialized = _serializer.Serialize(originalObject);
        var deserialized = _serializer.Deserialize(serialized, schema);

        // Assert
        Assert.Equal(originalObject.GetProperty<int>("vigor"), deserialized.GetProperty<int>("vigor"));
        Assert.Equal(originalObject.GetProperty<int>("maxVigor"), deserialized.GetProperty<int>("maxVigor"));
        Assert.Equal(originalObject.GetProperty<int>("vigorGen"), deserialized.GetProperty<int>("vigorGen"));
    }

    [Fact]
    public void Deserialize_WithElfVigorBytes_ReturnsCorrectValues()
    {
        // Arrange - The specific test case from the requirements
        var schema = CreateElfVigorSchema();
        var testBytes = new byte[] { 0x01, 0x00, 0x00, 0x00, 0x40, 0x00, 0x00, 0x00, 0x80, 0x00, 0x00, 0x00 };

        // Act
        var result = _serializer.Deserialize(testBytes, schema);

        // Assert - Expected: vigor=1, maxVigor=64, vigorGen=128
        Assert.Equal(1, result.GetProperty<int>("vigor"));
        Assert.Equal(64, result.GetProperty<int>("maxVigor"));
        Assert.Equal(128, result.GetProperty<int>("vigorGen"));
    }

    [Fact]
    public void Serialize_WithElfVigorValues_ReturnsCorrectBytes()
    {
        // Arrange - The specific test case from the requirements
        var schema = CreateElfVigorSchema();
        var gameDataObject = new DynamicGameDataObject(schema);
        gameDataObject.SetProperty("vigor", 1);
        gameDataObject.SetProperty("maxVigor", 64);
        gameDataObject.SetProperty("vigorGen", 128);

        // Act
        var result = _serializer.Serialize(gameDataObject);

        // Assert - Expected bytes: 01 00 00 00 40 00 00 00 80 00 00 00
        var expectedBytes = new byte[] { 0x01, 0x00, 0x00, 0x00, 0x40, 0x00, 0x00, 0x00, 0x80, 0x00, 0x00, 0x00 };
        Assert.Equal(expectedBytes, result);
    }

    [Fact]
    public void GetExpectedByteLength_WithElfVigorSchema_ReturnsCorrectLength()
    {
        // Arrange
        var schema = CreateElfVigorSchema();

        // Act
        var result = _serializer.GetExpectedByteLength(schema);

        // Assert - 3 int32 values = 12 bytes
        Assert.Equal(12, result);
    }

    [Fact]
    public void Serialize_WithNullObject_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _serializer.Serialize(null!));
    }

    [Fact]
    public void Deserialize_WithNullData_ThrowsArgumentNullException()
    {
        // Arrange
        var schema = CreateElfVigorSchema();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _serializer.Deserialize(null!, schema));
    }

    [Fact]
    public void Deserialize_WithNullSchema_ThrowsArgumentNullException()
    {
        // Arrange
        var data = new byte[] { 0x01, 0x00, 0x00, 0x00 };

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _serializer.Deserialize(data, null!));
    }

    [Fact]
    public void Serialize_WithUnsupportedType_ThrowsInvalidOperationException()
    {
        // Arrange
        var schema = new GameDataSchema
        {
            Name = "TestSchema",
            Type = "object",
            Properties = new Dictionary<string, PropertyDefinition>
            {
                ["unsupported"] = new PropertyDefinition { Type = "unsupported_type" }
            }
        };
        var gameDataObject = new DynamicGameDataObject(schema);
        gameDataObject.SetProperty("unsupported", "test");

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => _serializer.Serialize(gameDataObject));
    }

    [Fact]
    public void Deserialize_WithUnsupportedType_ThrowsInvalidOperationException()
    {
        // Arrange
        var schema = new GameDataSchema
        {
            Name = "TestSchema",
            Type = "object",
            Properties = new Dictionary<string, PropertyDefinition>
            {
                ["unsupported"] = new PropertyDefinition { Type = "unsupported_type" }
            }
        };
        var data = new byte[] { 0x01, 0x00, 0x00, 0x00 };

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => _serializer.Deserialize(data, schema));
    }

    [Fact]
    public void Serialize_WithDifferentNumericTypes_HandlesCorrectly()
    {
        // Arrange
        var schema = new GameDataSchema
        {
            Name = "NumericTestSchema",
            Type = "object",
            Properties = new Dictionary<string, PropertyDefinition>
            {
                ["intValue"] = new PropertyDefinition { Type = "int" },
                ["uintValue"] = new PropertyDefinition { Type = "uint" },
                ["shortValue"] = new PropertyDefinition { Type = "short" },
                ["ushortValue"] = new PropertyDefinition { Type = "ushort" },
                ["byteValue"] = new PropertyDefinition { Type = "byte" },
                ["sbyteValue"] = new PropertyDefinition { Type = "sbyte" },
                ["longValue"] = new PropertyDefinition { Type = "long" },
                ["ulongValue"] = new PropertyDefinition { Type = "ulong" },
                ["floatValue"] = new PropertyDefinition { Type = "float" },
                ["doubleValue"] = new PropertyDefinition { Type = "double" },
                ["boolValue"] = new PropertyDefinition { Type = "bool" }
            }
        };

        var gameDataObject = new DynamicGameDataObject(schema);
        gameDataObject.SetProperty("intValue", -1);
        gameDataObject.SetProperty("uintValue", 1u);
        gameDataObject.SetProperty("shortValue", (short)-2);
        gameDataObject.SetProperty("ushortValue", (ushort)2);
        gameDataObject.SetProperty("byteValue", (byte)3);
        gameDataObject.SetProperty("sbyteValue", (sbyte)-3);
        gameDataObject.SetProperty("longValue", -4L);
        gameDataObject.SetProperty("ulongValue", 4UL);
        gameDataObject.SetProperty("floatValue", 1.5f);
        gameDataObject.SetProperty("doubleValue", 2.5);
        gameDataObject.SetProperty("boolValue", true);

        // Act
        var serialized = _serializer.Serialize(gameDataObject);
        var deserialized = _serializer.Deserialize(serialized, schema);

        // Assert
        Assert.Equal(-1, deserialized.GetProperty<int>("intValue"));
        Assert.Equal(1u, deserialized.GetProperty<uint>("uintValue"));
        Assert.Equal((short)-2, deserialized.GetProperty<short>("shortValue"));
        Assert.Equal((ushort)2, deserialized.GetProperty<ushort>("ushortValue"));
        Assert.Equal((byte)3, deserialized.GetProperty<byte>("byteValue"));
        Assert.Equal((sbyte)-3, deserialized.GetProperty<sbyte>("sbyteValue"));
        Assert.Equal(-4L, deserialized.GetProperty<long>("longValue"));
        Assert.Equal(4UL, deserialized.GetProperty<ulong>("ulongValue"));
        Assert.Equal(1.5f, deserialized.GetProperty<float>("floatValue"));
        Assert.Equal(2.5, deserialized.GetProperty<double>("doubleValue"));
        Assert.True(deserialized.GetProperty<bool>("boolValue"));
    }

    [Fact]
    public void Serialize_WithStringType_HandlesCorrectly()
    {
        // Arrange
        var schema = new GameDataSchema
        {
            Name = "StringTestSchema",
            Type = "object",
            Properties = new Dictionary<string, PropertyDefinition>
            {
                ["stringValue"] = new PropertyDefinition { Type = "string" }
            }
        };

        var gameDataObject = new DynamicGameDataObject(schema);
        gameDataObject.SetProperty("stringValue", "Hello World");

        // Act
        var serialized = _serializer.Serialize(gameDataObject);
        var deserialized = _serializer.Deserialize(serialized, schema);

        // Assert
        Assert.Equal("Hello World", deserialized.GetProperty<string>("stringValue"));
    }

    [Fact]
    public void Serialize_WithEmptyString_HandlesCorrectly()
    {
        // Arrange
        var schema = new GameDataSchema
        {
            Name = "StringTestSchema",
            Type = "object",
            Properties = new Dictionary<string, PropertyDefinition>
            {
                ["stringValue"] = new PropertyDefinition { Type = "string" }
            }
        };

        var gameDataObject = new DynamicGameDataObject(schema);
        gameDataObject.SetProperty("stringValue", "");

        // Act
        var serialized = _serializer.Serialize(gameDataObject);
        var deserialized = _serializer.Deserialize(serialized, schema);

        // Assert
        Assert.Equal("", deserialized.GetProperty<string>("stringValue"));
    }

    /// <summary>
    /// Creates the ElfVigor schema for testing
    /// </summary>
    private GameDataSchema CreateElfVigorSchema()
    {
        return new GameDataSchema
        {
            Name = "ElfVigor",
            Type = "object",
            Properties = new Dictionary<string, PropertyDefinition>
            {
                ["vigor"] = new PropertyDefinition { Type = "int" },
                ["maxVigor"] = new PropertyDefinition { Type = "int" },
                ["vigorGen"] = new PropertyDefinition { Type = "int" }
            }
        };
    }
}