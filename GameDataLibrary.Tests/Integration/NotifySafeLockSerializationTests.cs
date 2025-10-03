using GameDataLibrary.Core.Models;
using GameDataLibrary.Core.Services;
using Xunit;

namespace GameDataLibrary.Tests.Integration;

public class NotifySafeLockSerializationTests
{
    private readonly GameDataSerializer _serializer;
    private readonly GameDataParser _parser;

    public NotifySafeLockSerializationTests()
    {
        _serializer = new GameDataSerializer();
        _parser = new GameDataParser();
    }

    [Fact]
    public async Task ParseNotifySafeLockSchema_FromYamlFile_ReturnsValidSchema()
    {
        // Arrange
        var yamlFilePath = GetNotifySafeLockSchemaPath();

        // Act
        var parsedData = await _parser.ParseFileAsync(yamlFilePath);

        // Assert
        Assert.True(parsedData.IsValid, $"Parsing failed: {parsedData.ErrorMessage}");
        Assert.NotNull(parsedData.Schema);
        Assert.Equal("NotifySafeLock", parsedData.Schema.Name);
        Assert.Equal("object", parsedData.Schema.Type);
        Assert.Equal(4, parsedData.Schema.Properties.Count);
        Assert.Contains("active", parsedData.Schema.Properties.Keys);
        Assert.Contains("time", parsedData.Schema.Properties.Keys);
        Assert.Contains("maxTime", parsedData.Schema.Properties.Keys);
        Assert.Contains("unknown", parsedData.Schema.Properties.Keys);
        
        // Verify property types
        Assert.Equal("byte", parsedData.Schema.Properties["active"].Type);
        Assert.Equal("timestamp", parsedData.Schema.Properties["time"].Type);
        Assert.Equal("int", parsedData.Schema.Properties["maxTime"].Type);
        Assert.Equal("byte", parsedData.Schema.Properties["unknown"].Type);
    }

    [Fact]
    public async Task DeserializeNotifySafeLock_WithSpecificByteArray_ReturnsCorrectValues()
    {
        // Arrange - Load schema from YAML file
        var yamlFilePath = GetNotifySafeLockSchemaPath();
        var parsedData = await _parser.ParseFileAsync(yamlFilePath);
        Assert.True(parsedData.IsValid);

        var schema = parsedData.Schema;
        // Bytes: 03 3b a6 df 68 3c 00 00 00 01
        var testBytes = new byte[] { 0x03, 0x3b, 0xa6, 0xdf, 0x68, 0x3c, 0x00, 0x00, 0x00, 0x01 };

        // Act
        var result = _serializer.Deserialize(testBytes, schema);

        // Assert - Expected: active=3, time='10/3/2025 10:32:27 AM', maxTime=60, unknown=1
        Assert.Equal(3, result.GetProperty<byte>("active"));
        
        var timeValue = result.GetProperty<DateTime>("time");
        // The timestamp 0x68dfa63b (1759487547) converts to 2025-10-03 10:32:27 UTC
        // The expected value is 10/3/2025 10:32:27 AM UTC
        var expectedTime = new DateTime(2025, 10, 3, 10, 32, 27, DateTimeKind.Utc);
        Assert.Equal(expectedTime, timeValue);
        
        Assert.Equal(60, result.GetProperty<int>("maxTime"));
        Assert.Equal(1, result.GetProperty<byte>("unknown"));
    }

    [Fact]
    public async Task SerializeNotifySafeLock_WithSpecificValues_ReturnsCorrectByteArray()
    {
        // Arrange - Load schema from YAML file
        var yamlFilePath = GetNotifySafeLockSchemaPath();
        var parsedData = await _parser.ParseFileAsync(yamlFilePath);
        Assert.True(parsedData.IsValid);

        var schema = parsedData.Schema;
        var gameDataObject = new DynamicGameDataObject(schema);
        gameDataObject.SetProperty("active", (byte)3);
        gameDataObject.SetProperty("time", new DateTime(2025, 10, 3, 10, 32, 27, DateTimeKind.Utc));
        gameDataObject.SetProperty("maxTime", 60);
        gameDataObject.SetProperty("unknown", (byte)1);

        // Act
        var result = _serializer.Serialize(gameDataObject);

        // Assert - Expected bytes: 03 3b a6 df 68 3c 00 00 00 01
        var expectedBytes = new byte[] { 0x03, 0x3b, 0xa6, 0xdf, 0x68, 0x3c, 0x00, 0x00, 0x00, 0x01 };
        Assert.Equal(expectedBytes, result);
    }

    [Fact]
    public async Task SerializeDeserializeNotifySafeLock_WithSpecificValues_WorksCorrectly()
    {
        // Arrange - Load schema from YAML file
        var yamlFilePath = GetNotifySafeLockSchemaPath();
        var parsedData = await _parser.ParseFileAsync(yamlFilePath);
        Assert.True(parsedData.IsValid);

        var schema = parsedData.Schema;
        var gameDataObject = new DynamicGameDataObject(schema);
        gameDataObject.SetProperty("active", (byte)3);
        gameDataObject.SetProperty("time", new DateTime(2025, 10, 3, 10, 32, 27, DateTimeKind.Utc));
        gameDataObject.SetProperty("maxTime", 60);
        gameDataObject.SetProperty("unknown", (byte)1);

        // Act - Serialize
        var serializedBytes = _serializer.Serialize(gameDataObject);

        // Act - Deserialize
        var deserializedObject = _serializer.Deserialize(serializedBytes, schema);

        // Assert - Check the specific values from the requirements
        Assert.Equal(3, deserializedObject.GetProperty<byte>("active"));
        Assert.Equal(new DateTime(2025, 10, 3, 10, 32, 27, DateTimeKind.Utc), deserializedObject.GetProperty<DateTime>("time"));
        Assert.Equal(60, deserializedObject.GetProperty<int>("maxTime"));
        Assert.Equal(1, deserializedObject.GetProperty<byte>("unknown"));

        // Assert - Check the specific byte array from the requirements
        var expectedBytes = new byte[] { 0x03, 0x3b, 0xa6, 0xdf, 0x68, 0x3c, 0x00, 0x00, 0x00, 0x01 };
        Assert.Equal(expectedBytes, serializedBytes);
    }

    [Fact]
    public async Task NotifySafeLockSchema_ExpectedByteLength_IsCorrect()
    {
        // Arrange - Load schema from YAML file
        var yamlFilePath = GetNotifySafeLockSchemaPath();
        var parsedData = await _parser.ParseFileAsync(yamlFilePath);
        Assert.True(parsedData.IsValid);

        var schema = parsedData.Schema;

        // Act
        var expectedLength = _serializer.GetExpectedByteLength(schema);

        // Assert - active(1) + time(4) + maxTime(4) + unknown(1) = 10 bytes
        Assert.Equal(10, expectedLength);
    }

    [Fact]
    public async Task NotifySafeLockSchema_RoundTripSerialization_PreservesData()
    {
        // Arrange - Load schema from YAML file
        var yamlFilePath = GetNotifySafeLockSchemaPath();
        var parsedData = await _parser.ParseFileAsync(yamlFilePath);
        Assert.True(parsedData.IsValid);

        var schema = parsedData.Schema;
        var originalObject = new DynamicGameDataObject(schema);
        originalObject.SetProperty("active", (byte)42);
        originalObject.SetProperty("time", new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc));
        originalObject.SetProperty("maxTime", 100);
        originalObject.SetProperty("unknown", (byte)255);

        // Act
        var serialized = _serializer.Serialize(originalObject);
        var deserialized = _serializer.Deserialize(serialized, schema);

        // Assert
        Assert.Equal(originalObject.GetProperty<byte>("active"), deserialized.GetProperty<byte>("active"));
        Assert.Equal(originalObject.GetProperty<DateTime>("time"), deserialized.GetProperty<DateTime>("time"));
        Assert.Equal(originalObject.GetProperty<int>("maxTime"), deserialized.GetProperty<int>("maxTime"));
        Assert.Equal(originalObject.GetProperty<byte>("unknown"), deserialized.GetProperty<byte>("unknown"));
    }

    [Fact]
    public async Task NotifySafeLockSchema_WithDifferentValues_SerializesCorrectly()
    {
        // Arrange - Load schema from YAML file
        var yamlFilePath = GetNotifySafeLockSchemaPath();
        var parsedData = await _parser.ParseFileAsync(yamlFilePath);
        Assert.True(parsedData.IsValid);

        var schema = parsedData.Schema;

        // Test with various values
        var testCases = new[]
        {
            new { active = (byte)0, time = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc), maxTime = 0, unknown = (byte)0 },
            new { active = (byte)255, time = new DateTime(2030, 12, 31, 23, 59, 59, DateTimeKind.Utc), maxTime = 1000, unknown = (byte)255 },
            new { active = (byte)128, time = new DateTime(2020, 6, 15, 14, 30, 45, DateTimeKind.Utc), maxTime = -100, unknown = (byte)128 }
        };

        foreach (var testCase in testCases)
        {
            // Arrange
            var gameDataObject = new DynamicGameDataObject(schema);
            gameDataObject.SetProperty("active", testCase.active);
            gameDataObject.SetProperty("time", testCase.time);
            gameDataObject.SetProperty("maxTime", testCase.maxTime);
            gameDataObject.SetProperty("unknown", testCase.unknown);

            // Act
            var serialized = _serializer.Serialize(gameDataObject);
            var deserialized = _serializer.Deserialize(serialized, schema);

            // Assert
            Assert.Equal(testCase.active, deserialized.GetProperty<byte>("active"));
            Assert.Equal(testCase.time, deserialized.GetProperty<DateTime>("time"));
            Assert.Equal(testCase.maxTime, deserialized.GetProperty<int>("maxTime"));
            Assert.Equal(testCase.unknown, deserialized.GetProperty<byte>("unknown"));
        }
    }

    [Fact]
    public async Task NotifySafeLockSchema_TimestampType_HandlesDateTimeCorrectly()
    {
        // Arrange - Load schema from YAML file
        var yamlFilePath = GetNotifySafeLockSchemaPath();
        var parsedData = await _parser.ParseFileAsync(yamlFilePath);
        Assert.True(parsedData.IsValid);

        var schema = parsedData.Schema;

        // Test timestamp serialization/deserialization
        var testTimes = new[]
        {
            new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc), // Unix epoch
            new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc), // Y2K
            new DateTime(2025, 10, 3, 17, 32, 27, DateTimeKind.Utc), // The specific test case
            new DateTime(2038, 1, 19, 3, 14, 7, DateTimeKind.Utc) // Near 32-bit timestamp limit
        };

        foreach (var testTime in testTimes)
        {
            // Arrange
            var gameDataObject = new DynamicGameDataObject(schema);
            gameDataObject.SetProperty("active", (byte)1);
            gameDataObject.SetProperty("time", testTime);
            gameDataObject.SetProperty("maxTime", 1);
            gameDataObject.SetProperty("unknown", (byte)1);

            // Act
            var serialized = _serializer.Serialize(gameDataObject);
            var deserialized = _serializer.Deserialize(serialized, schema);

            // Assert
            var deserializedTime = deserialized.GetProperty<DateTime>("time");
            Assert.Equal(testTime, deserializedTime);
        }
    }

    /// <summary>
    /// Gets the path to the notifysafelock.0x105.yml schema file
    /// </summary>
    /// <returns>Full path to the schema file</returns>
    private string GetNotifySafeLockSchemaPath()
    {
        // Try multiple possible paths
        var possiblePaths = new[]
        {
            Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "schema", "gamedata", "notifysafelock.0x105.yml"),
            Path.Combine(Directory.GetCurrentDirectory(), "schema", "gamedata", "notifysafelock.0x105.yml"),
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "schema", "gamedata", "notifysafelock.0x105.yml"),
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "schema", "gamedata", "notifysafelock.0x105.yml"),
            Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "..", "schema", "gamedata", "notifysafelock.0x105.yml"),
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "schema", "gamedata", "notifysafelock.0x105.yml")
        };

        foreach (var path in possiblePaths)
        {
            var fullPath = Path.GetFullPath(path);
            if (File.Exists(fullPath))
            {
                return fullPath;
            }
        }

        throw new FileNotFoundException($"Could not find notifysafelock.0x105.yml schema file. Tried paths: {string.Join(", ", possiblePaths)}");
    }
}