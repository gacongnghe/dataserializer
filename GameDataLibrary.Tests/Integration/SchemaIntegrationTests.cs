using GameDataLibrary.Core.Services;
using Xunit;

namespace GameDataLibrary.Tests.Integration;

public class SchemaIntegrationTests
{
    private readonly GameDataParser _parser;
    private readonly string _schemaDirectory;

    public SchemaIntegrationTests()
    {
        _parser = new GameDataParser();
        _schemaDirectory = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "..", "schema", "gamedata");
    }

    [Fact]
    public async Task ParseAllSchemaFiles_ShouldParseSuccessfully()
    {
        // Arrange
        if (!Directory.Exists(_schemaDirectory))
        {
            Assert.Fail($"Schema directory not found: {_schemaDirectory}");
            return;
        }

        // Act
        var results = await _parser.ParseDirectoryAsync(_schemaDirectory);

        // Assert
        Assert.NotEmpty(results);
        
        foreach (var result in results)
        {
            Assert.True(result.IsValid, $"Failed to parse {result.FileName}: {result.ErrorMessage}");
            Assert.NotNull(result.Schema);
            Assert.NotEmpty(result.Schema.Name);
            Assert.Equal("object", result.Schema.Type);
            Assert.NotEmpty(result.Schema.Properties);
        }
    }

    [Fact]
    public async Task ValidateAllSchemaFiles_ShouldAllBeValid()
    {
        // Arrange
        if (!Directory.Exists(_schemaDirectory))
        {
            Assert.Fail($"Schema directory not found: {_schemaDirectory}");
            return;
        }

        // Act
        var isValid = await _parser.ValidateAllFilesAsync(_schemaDirectory);

        // Assert
        Assert.True(isValid, "One or more schema files failed validation");
    }

    [Fact]
    public async Task ParseSpecificSchemaFiles_ShouldHaveCorrectStructure()
    {
        // Arrange
        if (!Directory.Exists(_schemaDirectory))
        {
            Assert.Fail($"Schema directory not found: {_schemaDirectory}");
            return;
        }

        // Act
        var results = await _parser.ParseDirectoryAsync(_schemaDirectory);
        var resultsList = results.ToList();

        // Assert - Check ElfVigor schema
        var elfVigorSchema = resultsList.FirstOrDefault(r => r.Schema.Name == "ElfVigor");
        if (elfVigorSchema != null)
        {
            Assert.True(elfVigorSchema.IsValid);
            Assert.Contains("vigor", elfVigorSchema.Schema.Properties.Keys);
            Assert.Contains("maxVigor", elfVigorSchema.Schema.Properties.Keys);
            Assert.Contains("vigorGen", elfVigorSchema.Schema.Properties.Keys);
            Assert.Equal("int", elfVigorSchema.Schema.Properties["vigor"].Type);
            Assert.Equal("int", elfVigorSchema.Schema.Properties["maxVigor"].Type);
            Assert.Equal("int", elfVigorSchema.Schema.Properties["vigorGen"].Type);
        }

        // Assert - Check PlayerInfo schema
        var playerInfoSchema = resultsList.FirstOrDefault(r => r.Schema.Name == "PlayerInfo");
        if (playerInfoSchema != null)
        {
            Assert.True(playerInfoSchema.IsValid);
            Assert.Contains("id", playerInfoSchema.Schema.Properties.Keys);
            Assert.Contains("position", playerInfoSchema.Schema.Properties.Keys);
            Assert.Contains("crc", playerInfoSchema.Schema.Properties.Keys);
            Assert.Equal("uint", playerInfoSchema.Schema.Properties["id"].Type);
            Assert.Equal("point3f", playerInfoSchema.Schema.Properties["position"].Type);
            Assert.Equal("ushort", playerInfoSchema.Schema.Properties["crc"].Type);
        }

        // Assert - Check PlayerInfo1List schema
        var playerInfoListSchema = resultsList.FirstOrDefault(r => r.Schema.Name == "PlayerInfo1List");
        if (playerInfoListSchema != null)
        {
            Assert.True(playerInfoListSchema.IsValid);
            Assert.Contains("players", playerInfoListSchema.Schema.Properties.Keys);
            Assert.Equal("array", playerInfoListSchema.Schema.Properties["players"].Type);
            Assert.NotNull(playerInfoListSchema.Schema.Properties["players"].Meta);
            Assert.True(playerInfoListSchema.Schema.Properties["players"].Meta!.ContainsKey("size"));
            Assert.True(playerInfoListSchema.Schema.Properties["players"].Meta!.ContainsKey("itemType"));
        }
    }

    [Fact]
    public async Task GetValidationSummary_ShouldProvideDetailedResults()
    {
        // Arrange
        if (!Directory.Exists(_schemaDirectory))
        {
            Assert.Fail($"Schema directory not found: {_schemaDirectory}");
            return;
        }

        // Act
        var summary = await _parser.GetValidationSummaryAsync(_schemaDirectory);

        // Assert
        Assert.True(summary.TotalFiles > 0, "Should have found at least one YAML file");
        Assert.Equal(summary.TotalFiles, summary.ValidFiles);
        Assert.Equal(0, summary.InvalidFiles);
        Assert.True(summary.AllValid);
        Assert.Equal(summary.TotalFiles, summary.Results.Count);
        
        // Verify each result has proper metadata
        foreach (var result in summary.Results)
        {
            Assert.NotNull(result.FileName);
            Assert.NotNull(result.FilePath);
            Assert.NotEqual(default(DateTime), result.ParsedAt);
        }
    }

    [Fact]
    public async Task SchemaFiles_ShouldContainExpectedFiles()
    {
        // Arrange
        if (!Directory.Exists(_schemaDirectory))
        {
            Assert.Fail($"Schema directory not found: {_schemaDirectory}");
            return;
        }

        // Act
        var results = await _parser.ParseDirectoryAsync(_schemaDirectory);
        var fileNames = results.Select(r => r.FileName).ToList();

        // Assert - Check that expected files exist
        var expectedFiles = new[] { "elfvigor.0x106.yml", "playerinfo.0x00.yml", "playerinfo1list.0x04.yml" };
        
        foreach (var expectedFile in expectedFiles)
        {
            Assert.Contains(expectedFile, fileNames);
        }
    }
}