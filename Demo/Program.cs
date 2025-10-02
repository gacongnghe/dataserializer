using GameDataLibrary.Core.Services;
using GameDataLibrary.Core.Models;

// Create serializer and parser
var serializer = new GameDataSerializer();
var parser = new GameDataParser();

// Load the elfvigor schema from YAML
var yamlFilePath = Path.Combine(Directory.GetCurrentDirectory(), "..", "schema", "gamedata", "elfvigor.0x106.yml");
var parsedData = await parser.ParseFileAsync(yamlFilePath);

if (!parsedData.IsValid)
{
    Console.WriteLine($"Failed to parse schema: {parsedData.ErrorMessage}");
    return;
}

var schema = parsedData.Schema;
Console.WriteLine($"Loaded schema: {schema.Name}");
Console.WriteLine($"Properties: {string.Join(", ", schema.Properties.Keys)}");
Console.WriteLine();

// Test the specific scenario from the requirements
Console.WriteLine("=== Testing Specific Scenario ===");
Console.WriteLine("Expected: vigor=1, maxVigor=64, vigorGen=128");
Console.WriteLine("Expected bytes: 01 00 00 00 40 00 00 00 80 00 00 00");
Console.WriteLine();

// Create object with the specific values
var gameDataObject = new DynamicGameDataObject(schema);
gameDataObject.SetProperty("vigor", 1);
gameDataObject.SetProperty("maxVigor", 64);
gameDataObject.SetProperty("vigorGen", 128);

Console.WriteLine($"Created object: {gameDataObject}");
Console.WriteLine();

// Serialize to bytes
var serializedBytes = serializer.Serialize(gameDataObject);
Console.WriteLine($"Serialized bytes: {BitConverter.ToString(serializedBytes).Replace("-", " ")}");
Console.WriteLine($"Expected bytes:   01 00 00 00 40 00 00 00 80 00 00 00");
Console.WriteLine($"Match: {BitConverter.ToString(serializedBytes).Replace("-", " ") == "01 00 00 00 40 00 00 00 80 00 00 00"}");
Console.WriteLine();

// Deserialize from bytes
var testBytes = new byte[] { 0x01, 0x00, 0x00, 0x00, 0x40, 0x00, 0x00, 0x00, 0x80, 0x00, 0x00, 0x00 };
var deserializedObject = serializer.Deserialize(testBytes, schema);

Console.WriteLine($"Deserialized object: {deserializedObject}");
Console.WriteLine($"vigor: {deserializedObject.GetProperty<int>("vigor")}");
Console.WriteLine($"maxVigor: {deserializedObject.GetProperty<int>("maxVigor")}");
Console.WriteLine($"vigorGen: {deserializedObject.GetProperty<int>("vigorGen")}");
Console.WriteLine();

// Test round-trip
Console.WriteLine("=== Testing Round-Trip ===");
var originalObject = new DynamicGameDataObject(schema);
originalObject.SetProperty("vigor", 42);
originalObject.SetProperty("maxVigor", 100);
originalObject.SetProperty("vigorGen", 200);

var roundTripBytes = serializer.Serialize(originalObject);
var roundTripObject = serializer.Deserialize(roundTripBytes, schema);

Console.WriteLine($"Original: {originalObject}");
Console.WriteLine($"Round-trip: {roundTripObject}");
Console.WriteLine($"Match: {originalObject.GetProperty<int>("vigor") == roundTripObject.GetProperty<int>("vigor") && 
                          originalObject.GetProperty<int>("maxVigor") == roundTripObject.GetProperty<int>("maxVigor") && 
                          originalObject.GetProperty<int>("vigorGen") == roundTripObject.GetProperty<int>("vigorGen")}");
Console.WriteLine();

// Show expected byte length
var expectedLength = serializer.GetExpectedByteLength(schema);
Console.WriteLine($"Expected byte length for {schema.Name}: {expectedLength} bytes");
Console.WriteLine($"Actual serialized length: {serializedBytes.Length} bytes");