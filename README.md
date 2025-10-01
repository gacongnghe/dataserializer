# GameDataLibrary

[![CI](https://github.com/your-username/GameDataLibrary/workflows/CI/badge.svg)](https://github.com/your-username/GameDataLibrary/actions/workflows/ci.yml)
[![Build and Test](https://github.com/your-username/GameDataLibrary/workflows/Build%20and%20Test/badge.svg)](https://github.com/your-username/GameDataLibrary/actions/workflows/build-and-test.yml)

A .NET Core library for parsing YAML game data schema files with comprehensive testing and validation capabilities.

## Features

- **YAML Parsing**: Parse YAML schema definition files using YamlDotNet
- **Data Models**: Strongly-typed models for game data schemas
- **Validation**: Comprehensive validation of YAML files with detailed error reporting
- **Testing**: Full test coverage including unit tests and integration tests
- **Extensions**: Convenient extension methods for common operations

## Project Structure

```
GameDataLibrary/
├── GameDataLibrary.Core/           # Main library
│   ├── Models/                     # Data models
│   │   └── GameDataSchema.cs      # Schema and property definitions
│   ├── Services/                   # Core services
│   │   └── GameDataParser.cs      # YAML parsing service
│   └── GameDataParserExtensions.cs # Extension methods
├── GameDataLibrary.Tests/          # Test project
│   ├── Models/                     # Model tests
│   ├── Services/                   # Service tests
│   └── Integration/                # Integration tests
└── schema/gamedata/                # Sample YAML schema files
    ├── elfvigor.0x106.yml
    ├── playerinfo.0x00.yml
    └── playerinfo1list.0x04.yml
```

## Data Models

### GameDataSchema
Represents a game data schema definition with:
- `Type`: Schema type (typically "object")
- `Name`: Schema name
- `Properties`: Dictionary of property definitions

### PropertyDefinition
Represents a property within a schema with:
- `Type`: Property data type
- `Meta`: Optional metadata (size, encoding, itemType, etc.)

### ParsedGameData
Contains parsing results with:
- `FileName`: Name of the parsed file
- `FilePath`: Full path to the file
- `Schema`: Parsed schema data
- `IsValid`: Whether parsing succeeded
- `ErrorMessage`: Error details if parsing failed
- `ParsedAt`: Timestamp of parsing

## Usage

### Basic Usage

```csharp
using GameDataLibrary.Core.Services;

var parser = new GameDataParser();

// Parse a single file
var result = await parser.ParseFileAsync("path/to/schema.yml");
if (result.IsValid)
{
    Console.WriteLine($"Schema: {result.Schema.Name}");
    foreach (var property in result.Schema.Properties)
    {
        Console.WriteLine($"  {property.Key}: {property.Value.Type}");
    }
}

// Parse all files in a directory
var results = await parser.ParseDirectoryAsync("schema/gamedata");
foreach (var parsedData in results)
{
    Console.WriteLine($"{parsedData.FileName}: {(parsedData.IsValid ? "Valid" : "Invalid")}");
}
```

### Validation

```csharp
// Validate all files
var allValid = await parser.ValidateAllFilesAsync("schema/gamedata");
Console.WriteLine($"All files valid: {allValid}");

// Get detailed validation summary
var summary = await parser.GetValidationSummaryAsync("schema/gamedata");
Console.WriteLine($"Total: {summary.TotalFiles}, Valid: {summary.ValidFiles}, Invalid: {summary.InvalidFiles}");
```

### Extension Methods

```csharp
// Parse default schema directory
var results = await parser.ParseDefaultSchemaDirectoryAsync();

// Validate default schema directory
var isValid = await parser.ValidateDefaultSchemaDirectoryAsync();

// Get formatted summary
var summary = results.GetSummary();
Console.WriteLine(summary);
```

## Sample YAML Schema

```yaml
type: object
name: PlayerInfo
properties:
  id:
    type: uint
  position:
    type: point3f
  crc:
    type: ushort
  customCrc:
    type: ushort
  dir:
    type: byte
  objectState1:
    type: uint
  objectState2:
    type: uint
```

## Testing

The library includes comprehensive tests:

- **Unit Tests**: Test individual components and methods
- **Integration Tests**: Test parsing of actual YAML files in schema/gamedata
- **Model Tests**: Test data model behavior and serialization

Run tests with:
```bash
dotnet test
```

## Dependencies

- **YamlDotNet**: For YAML parsing and deserialization
- **xUnit**: For testing framework
- **.NET 8.0**: Target framework

## Building

```bash
# Build the solution
dotnet build

# Run tests
dotnet test

# Run tests with verbose output
dotnet test --verbosity normal
```

## CI/CD Pipeline

The project includes GitHub Actions workflows for automated testing and validation:

### CI Pipeline (`ci.yml`)
- Runs on every push and pull request
- Builds the solution in Release configuration
- Runs all tests and validates YAML schemas
- **Pipeline passes only when all tests pass**

### Build and Test Pipeline (`build-and-test.yml`)
- Simple build and test workflow
- Runs on Linux with .NET 8.0
- **Pipeline passes only when all tests pass**

### Test Pipeline (`test.yml`)
- Dedicated test workflow with verification
- Shows test count and results
- **Pipeline passes only when all 20 tests pass**

### YAML Schema Validation (`validate-schemas.yml`)
- Validates YAML schema files when they change
- Runs integration tests specifically for schema parsing
- **Pipeline passes only when all tests pass**

### Pipeline Status
The build status is shown with badges at the top of this README. The pipeline will:
- ✅ **Pass** when all tests pass
- ❌ **Fail** when any test fails or build errors occur
- 🔄 **Show progress** with real-time status updates

## Test Results

All tests pass successfully:
- 20 total tests
- 100% pass rate
- Covers unit tests, integration tests, and model validation
- Validates parsing of all existing YAML schema files in `schema/gamedata/`

## License

This project is provided as-is for educational and development purposes.