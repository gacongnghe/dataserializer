using GameDataLibrary.Core.Models;
using GameDataLibrary.Core.Services;
using YamlDotNet.Serialization;

namespace GameDataLibrary.Tests.Models;

public class GameDataSchemaTests
{
    [Fact]
    public void GameDataSchema_Properties_ShouldBeInitialized()
    {
        // Act
        var schema = new GameDataSchema();

        // Assert
        Assert.NotNull(schema.Type);
        Assert.NotNull(schema.Name);
        Assert.NotNull(schema.Properties);
        Assert.Empty(schema.Properties);
    }

    [Fact]
    public void PropertyDefinition_Properties_ShouldBeInitialized()
    {
        // Act
        var property = new PropertyDefinition();

        // Assert
        Assert.NotNull(property.Type);
        Assert.Null(property.Meta);
    }

    [Fact]
    public void ParsedGameData_Properties_ShouldBeInitialized()
    {
        // Act
        var parsedData = new ParsedGameData();

        // Assert
        Assert.NotNull(parsedData.FileName);
        Assert.NotNull(parsedData.FilePath);
        Assert.NotNull(parsedData.Schema);
        Assert.False(parsedData.IsValid);
        Assert.Null(parsedData.ErrorMessage);
        Assert.NotEqual(default(DateTime), parsedData.ParsedAt);
    }

    [Fact]
    public void GameDataSchema_YamlDeserialization_ShouldWork()
    {
        // Arrange
        var yaml = @"
type: object
name: TestSchema
properties:
  id:
    type: uint
  name:
    type: string
  metadata:
    type: object
    meta:
      size: 10
      encoding: utf-8
";
        var deserializer = new DeserializerBuilder().Build();

        // Act
        var schema = deserializer.Deserialize<GameDataSchema>(yaml);

        // Assert
        Assert.Equal("object", schema.Type);
        Assert.Equal("TestSchema", schema.Name);
        Assert.Equal(3, schema.Properties.Count);
        
        Assert.Contains("id", schema.Properties.Keys);
        Assert.Equal("uint", schema.Properties["id"].Type);
        Assert.Null(schema.Properties["id"].Meta);
        
        Assert.Contains("name", schema.Properties.Keys);
        Assert.Equal("string", schema.Properties["name"].Type);
        Assert.Null(schema.Properties["name"].Meta);
        
        Assert.Contains("metadata", schema.Properties.Keys);
        Assert.Equal("object", schema.Properties["metadata"].Type);
        Assert.NotNull(schema.Properties["metadata"].Meta);
        Assert.Equal(2, schema.Properties["metadata"].Meta!.Count);
        Assert.True(schema.Properties["metadata"].Meta!.ContainsKey("size"));
        Assert.True(schema.Properties["metadata"].Meta!.ContainsKey("encoding"));
    }

    [Fact]
    public void ValidationSummary_Properties_ShouldBeInitialized()
    {
        // Act
        var summary = new ValidationSummary();

        // Assert
        Assert.Equal(0, summary.TotalFiles);
        Assert.Equal(0, summary.ValidFiles);
        Assert.Equal(0, summary.InvalidFiles);
        Assert.NotNull(summary.Results);
        Assert.Empty(summary.Results);
        Assert.True(summary.AllValid);
    }

    [Fact]
    public void ValidationSummary_AllValid_ShouldReturnTrueWhenNoInvalidFiles()
    {
        // Arrange
        var summary = new ValidationSummary
        {
            TotalFiles = 5,
            ValidFiles = 5,
            InvalidFiles = 0,
            Results = new List<ParsedGameData>()
        };

        // Act & Assert
        Assert.True(summary.AllValid);
    }

    [Fact]
    public void ValidationSummary_AllValid_ShouldReturnFalseWhenInvalidFilesExist()
    {
        // Arrange
        var summary = new ValidationSummary
        {
            TotalFiles = 5,
            ValidFiles = 3,
            InvalidFiles = 2,
            Results = new List<ParsedGameData>()
        };

        // Act & Assert
        Assert.False(summary.AllValid);
    }
}