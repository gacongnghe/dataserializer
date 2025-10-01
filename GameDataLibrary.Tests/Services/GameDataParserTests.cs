using GameDataLibrary.Core.Models;
using GameDataLibrary.Core.Services;
using Xunit;

namespace GameDataLibrary.Tests.Services;

public class GameDataParserTests : IDisposable
{
    private readonly GameDataParser _parser;
    private readonly string _testDirectory;

    public GameDataParserTests()
    {
        _parser = new GameDataParser();
        _testDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_testDirectory);
    }

    [Fact]
    public async Task ParseFileAsync_ValidYamlFile_ReturnsValidResult()
    {
        // Arrange
        var testYaml = @"
type: object
name: TestObject
properties:
  id:
    type: uint
  name:
    type: string
";
        var testFile = Path.Combine(_testDirectory, "test.yml");
        await File.WriteAllTextAsync(testFile, testYaml);

        // Act
        var result = await _parser.ParseFileAsync(testFile);

        // Assert
        Assert.True(result.IsValid);
        Assert.Equal("test.yml", result.FileName);
        Assert.Equal("object", result.Schema.Type);
        Assert.Equal("TestObject", result.Schema.Name);
        Assert.Equal(2, result.Schema.Properties.Count);
        Assert.Contains("id", result.Schema.Properties.Keys);
        Assert.Contains("name", result.Schema.Properties.Keys);
        Assert.Equal("uint", result.Schema.Properties["id"].Type);
        Assert.Equal("string", result.Schema.Properties["name"].Type);
    }

    [Fact]
    public async Task ParseFileAsync_InvalidYamlFile_ReturnsInvalidResult()
    {
        // Arrange
        var invalidYaml = @"
type: object
name: TestObject
properties:
  invalid: [unclosed bracket
";
        var testFile = Path.Combine(_testDirectory, "invalid.yml");
        await File.WriteAllTextAsync(testFile, invalidYaml);

        // Act
        var result = await _parser.ParseFileAsync(testFile);

        // Assert
        Assert.False(result.IsValid);
        Assert.NotNull(result.ErrorMessage);
        Assert.Equal("invalid.yml", result.FileName);
    }

    [Fact]
    public async Task ParseFileAsync_NonExistentFile_ReturnsInvalidResult()
    {
        // Arrange
        var nonExistentFile = Path.Combine(_testDirectory, "nonexistent.yml");

        // Act
        var result = await _parser.ParseFileAsync(nonExistentFile);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("File not found", result.ErrorMessage);
    }

    [Fact]
    public async Task ParseDirectoryAsync_ValidDirectory_ReturnsAllFiles()
    {
        // Arrange
        var yaml1 = @"
type: object
name: Object1
properties:
  id:
    type: uint
";
        var yaml2 = @"
type: object
name: Object2
properties:
  name:
    type: string
";
        
        await File.WriteAllTextAsync(Path.Combine(_testDirectory, "file1.yml"), yaml1);
        await File.WriteAllTextAsync(Path.Combine(_testDirectory, "file2.yml"), yaml2);

        // Act
        var results = await _parser.ParseDirectoryAsync(_testDirectory);

        // Assert
        Assert.Equal(2, results.Count());
        Assert.All(results, r => Assert.True(r.IsValid));
        Assert.Contains(results, r => r.Schema.Name == "Object1");
        Assert.Contains(results, r => r.Schema.Name == "Object2");
    }

    [Fact]
    public async Task ParseDirectoryAsync_NonExistentDirectory_ThrowsException()
    {
        // Arrange
        var nonExistentDir = Path.Combine(_testDirectory, "nonexistent");

        // Act & Assert
        await Assert.ThrowsAsync<DirectoryNotFoundException>(() => _parser.ParseDirectoryAsync(nonExistentDir));
    }

    [Fact]
    public async Task ValidateAllFilesAsync_AllValidFiles_ReturnsTrue()
    {
        // Arrange
        var validYaml = @"
type: object
name: ValidObject
properties:
  id:
    type: uint
";
        await File.WriteAllTextAsync(Path.Combine(_testDirectory, "valid1.yml"), validYaml);
        await File.WriteAllTextAsync(Path.Combine(_testDirectory, "valid2.yml"), validYaml);

        // Act
        var isValid = await _parser.ValidateAllFilesAsync(_testDirectory);

        // Assert
        Assert.True(isValid);
    }

    [Fact]
    public async Task ValidateAllFilesAsync_SomeInvalidFiles_ReturnsFalse()
    {
        // Arrange
        var validYaml = @"
type: object
name: ValidObject
properties:
  id:
    type: uint
";
        var invalidYaml = "invalid: [yaml content";
        
        await File.WriteAllTextAsync(Path.Combine(_testDirectory, "valid.yml"), validYaml);
        await File.WriteAllTextAsync(Path.Combine(_testDirectory, "invalid.yml"), invalidYaml);

        // Act
        var isValid = await _parser.ValidateAllFilesAsync(_testDirectory);

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public async Task GetValidationSummaryAsync_ReturnsCorrectSummary()
    {
        // Arrange
        var validYaml = @"
type: object
name: ValidObject
properties:
  id:
    type: uint
";
        var invalidYaml = "invalid: [yaml content";
        
        await File.WriteAllTextAsync(Path.Combine(_testDirectory, "valid.yml"), validYaml);
        await File.WriteAllTextAsync(Path.Combine(_testDirectory, "invalid.yml"), invalidYaml);

        // Act
        var summary = await _parser.GetValidationSummaryAsync(_testDirectory);

        // Assert
        Assert.Equal(2, summary.TotalFiles);
        Assert.Equal(1, summary.ValidFiles);
        Assert.Equal(1, summary.InvalidFiles);
        Assert.False(summary.AllValid);
        Assert.Equal(2, summary.Results.Count);
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDirectory))
        {
            Directory.Delete(_testDirectory, true);
        }
    }
}