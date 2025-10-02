using GameDataLibrary.Core.Models;
using GameDataLibrary.Core.Services;
using Xunit;

namespace GameDataLibrary.Tests.Integration;

public class ElfVigorSerializationTests
{
    private readonly GameDataSerializer _serializer;
    private readonly GameDataParser _parser;

    public ElfVigorSerializationTests()
    {
        _serializer = new GameDataSerializer();
        _parser = new GameDataParser();
    }

    [Fact]
    public async Task ParseElfVigorSchema_FromYamlFile_ReturnsValidSchema()
    {
        // Arrange
        var yamlFilePath = GetElfVigorSchemaPath();

        // Act
        var parsedData = await _parser.ParseFileAsync(yamlFilePath);

        // Assert
        Assert.True(parsedData.IsValid, $"Parsing failed: {parsedData.ErrorMessage}");
        Assert.NotNull(parsedData.Schema);
        Assert.Equal("ElfVigor", parsedData.Schema.Name);
        Assert.Equal("object", parsedData.Schema.Type);
        Assert.Equal(3, parsedData.Schema.Properties.Count);
        Assert.Contains("vigor", parsedData.Schema.Properties.Keys);
        Assert.Contains("maxVigor", parsedData.Schema.Properties.Keys);
        Assert.Contains("vigorGen", parsedData.Schema.Properties.Keys);
    }

    [Fact]
    public async Task SerializeDeserializeElfVigor_WithSpecificValues_WorksCorrectly()
    {
        // Arrange - Load schema from YAML file
        var yamlFilePath = GetElfVigorSchemaPath();
        var parsedData = await _parser.ParseFileAsync(yamlFilePath);
        Assert.True(parsedData.IsValid);

        var schema = parsedData.Schema;
        var gameDataObject = new DynamicGameDataObject(schema);
        gameDataObject.SetProperty("vigor", 1);
        gameDataObject.SetProperty("maxVigor", 64);
        gameDataObject.SetProperty("vigorGen", 128);

        // Act - Serialize
        var serializedBytes = _serializer.Serialize(gameDataObject);

        // Act - Deserialize
        var deserializedObject = _serializer.Deserialize(serializedBytes, schema);

        // Assert - Check the specific values from the requirements
        Assert.Equal(1, deserializedObject.GetProperty<int>("vigor"));
        Assert.Equal(64, deserializedObject.GetProperty<int>("maxVigor"));
        Assert.Equal(128, deserializedObject.GetProperty<int>("vigorGen"));

        // Assert - Check the specific byte array from the requirements
        var expectedBytes = new byte[] { 0x01, 0x00, 0x00, 0x00, 0x40, 0x00, 0x00, 0x00, 0x80, 0x00, 0x00, 0x00 };
        Assert.Equal(expectedBytes, serializedBytes);
    }

    [Fact]
    public async Task DeserializeElfVigor_WithSpecificByteArray_ReturnsCorrectValues()
    {
        // Arrange - Load schema from YAML file
        var yamlFilePath = GetElfVigorSchemaPath();
        var parsedData = await _parser.ParseFileAsync(yamlFilePath);
        Assert.True(parsedData.IsValid);

        var schema = parsedData.Schema;
        var testBytes = new byte[] { 0x01, 0x00, 0x00, 0x00, 0x40, 0x00, 0x00, 0x00, 0x80, 0x00, 0x00, 0x00 };

        // Act
        var result = _serializer.Deserialize(testBytes, schema);

        // Assert - Expected: vigor=1, maxVigor=64, vigorGen=128
        Assert.Equal(1, result.GetProperty<int>("vigor"));
        Assert.Equal(64, result.GetProperty<int>("maxVigor"));
        Assert.Equal(128, result.GetProperty<int>("vigorGen"));
    }

    [Fact]
    public async Task SerializeElfVigor_WithSpecificValues_ReturnsCorrectByteArray()
    {
        // Arrange - Load schema from YAML file
        var yamlFilePath = GetElfVigorSchemaPath();
        var parsedData = await _parser.ParseFileAsync(yamlFilePath);
        Assert.True(parsedData.IsValid);

        var schema = parsedData.Schema;
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
    public async Task ElfVigorSchema_ExpectedByteLength_IsCorrect()
    {
        // Arrange - Load schema from YAML file
        var yamlFilePath = GetElfVigorSchemaPath();
        var parsedData = await _parser.ParseFileAsync(yamlFilePath);
        Assert.True(parsedData.IsValid);

        var schema = parsedData.Schema;

        // Act
        var expectedLength = _serializer.GetExpectedByteLength(schema);

        // Assert - 3 int32 values = 12 bytes
        Assert.Equal(12, expectedLength);
    }

    [Fact]
    public async Task ElfVigorSchema_RoundTripSerialization_PreservesData()
    {
        // Arrange - Load schema from YAML file
        var yamlFilePath = GetElfVigorSchemaPath();
        var parsedData = await _parser.ParseFileAsync(yamlFilePath);
        Assert.True(parsedData.IsValid);

        var schema = parsedData.Schema;
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
    public async Task ElfVigorSchema_WithDifferentValues_SerializesCorrectly()
    {
        // Arrange - Load schema from YAML file
        var yamlFilePath = GetElfVigorSchemaPath();
        var parsedData = await _parser.ParseFileAsync(yamlFilePath);
        Assert.True(parsedData.IsValid);

        var schema = parsedData.Schema;

        // Test with various values
        var testCases = new[]
        {
            new { vigor = 0, maxVigor = 0, vigorGen = 0 },
            new { vigor = 255, maxVigor = 1000, vigorGen = 500 },
            new { vigor = -1, maxVigor = -100, vigorGen = -50 },
            new { vigor = int.MaxValue, maxVigor = int.MinValue, vigorGen = 0 }
        };

        foreach (var testCase in testCases)
        {
            // Arrange
            var gameDataObject = new DynamicGameDataObject(schema);
            gameDataObject.SetProperty("vigor", testCase.vigor);
            gameDataObject.SetProperty("maxVigor", testCase.maxVigor);
            gameDataObject.SetProperty("vigorGen", testCase.vigorGen);

            // Act
            var serialized = _serializer.Serialize(gameDataObject);
            var deserialized = _serializer.Deserialize(serialized, schema);

            // Assert
            Assert.Equal(testCase.vigor, deserialized.GetProperty<int>("vigor"));
            Assert.Equal(testCase.maxVigor, deserialized.GetProperty<int>("maxVigor"));
            Assert.Equal(testCase.vigorGen, deserialized.GetProperty<int>("vigorGen"));
        }
    }

    /// <summary>
    /// Gets the path to the elfvigor.0x106.yml schema file
    /// </summary>
    /// <returns>Full path to the schema file</returns>
    private string GetElfVigorSchemaPath()
    {
        // Try multiple possible paths
        var possiblePaths = new[]
        {
            Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "schema", "gamedata", "elfvigor.0x106.yml"),
            Path.Combine(Directory.GetCurrentDirectory(), "schema", "gamedata", "elfvigor.0x106.yml"),
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "schema", "gamedata", "elfvigor.0x106.yml"),
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "schema", "gamedata", "elfvigor.0x106.yml"),
            Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "..", "schema", "gamedata", "elfvigor.0x106.yml"),
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "schema", "gamedata", "elfvigor.0x106.yml")
        };

        foreach (var path in possiblePaths)
        {
            var fullPath = Path.GetFullPath(path);
            if (File.Exists(fullPath))
            {
                return fullPath;
            }
        }

        throw new FileNotFoundException($"Could not find elfvigor.0x106.yml schema file. Tried paths: {string.Join(", ", possiblePaths)}");
    }
}