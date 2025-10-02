using GameDataLibrary.Core.Models;
using GameDataLibrary.Core.Services;
using Xunit;

namespace GameDataLibrary.Tests.Integration;

public class PlayerInfoSerializationTests
{
    private readonly GameDataSerializer _serializer;
    private readonly GameDataParser _parser;

    public PlayerInfoSerializationTests()
    {
        _serializer = new GameDataSerializer();
        _parser = new GameDataParser();
    }

    [Fact]
    public void Deserialize_PlayerInfo_WithProvidedBytes_ReturnsCorrectObject()
    {
        // Arrange - The test bytes from the user's request
        var testBytes = new byte[] { 
            0x01, 0x00, 0xac, 0x12, 0xbd, 0x00, 0x00, 0x00, 0x80, 0x3f, 0x00, 0x00, 0x00, 0x40, 0x00, 0x00, 0xc0, 0x3f, 
            0x01, 0x00, 0x02, 0x00, 0x03, 0x01, 0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00 
        };

        // Register the schemas
        var playerInfoSchema = CreatePlayerInfoSchema();
        var playerInfoListSchema = CreatePlayerInfoListSchema();
        
        _serializer.RegisterSchema(playerInfoSchema);
        _serializer.RegisterSchema(playerInfoListSchema);

        // Act
        var result = _serializer.Deserialize(testBytes, playerInfoListSchema);

        // Assert
        Assert.NotNull(result);
        var players = result.GetProperty<List<object>>("players");
        Assert.NotNull(players);
        Assert.Single(players);

        var player = players[0] as DynamicGameDataObject;
        Assert.NotNull(player);
        
        // Expected values from the user's description:
        // id=0xbd12ac, position={x=1.0,z=2.0,y=1.5}, crc=1, customCrc=2, dir=3, objectState1=1, objectState2=3
        Assert.Equal(0xbd12acu, player.GetProperty<uint>("id"));
        
        var position = player.GetProperty<Point3F>("position");
        Assert.NotNull(position);
        Assert.Equal(1.0f, position.X);
        Assert.Equal(2.0f, position.Z);
        Assert.Equal(1.5f, position.Y);
        
        Assert.Equal(1, player.GetProperty<ushort>("crc"));
        Assert.Equal(2, player.GetProperty<ushort>("customCrc"));
        Assert.Equal(3, player.GetProperty<byte>("dir"));
        Assert.Equal(1u, player.GetProperty<uint>("objectState1"));
        Assert.Equal(2u, player.GetProperty<uint>("objectState2"));
    }

    [Fact]
    public void Serialize_PlayerInfo_WithProvidedValues_ReturnsCorrectBytes()
    {
        // Arrange
        var playerInfoSchema = CreatePlayerInfoSchema();
        var playerInfoListSchema = CreatePlayerInfoListSchema();
        
        _serializer.RegisterSchema(playerInfoSchema);
        _serializer.RegisterSchema(playerInfoListSchema);

        var playerInfo = new DynamicGameDataObject(playerInfoSchema);
        playerInfo.SetProperty("id", 0xbd12acu);
        playerInfo.SetProperty("position", new Point3F(1.0f, 2.0f, 1.5f));
        playerInfo.SetProperty("crc", (ushort)1);
        playerInfo.SetProperty("customCrc", (ushort)2);
        playerInfo.SetProperty("dir", (byte)3);
        playerInfo.SetProperty("objectState1", 1u);
        playerInfo.SetProperty("objectState2", 2u);

        var playerInfoList = new DynamicGameDataObject(playerInfoListSchema);
        playerInfoList.SetProperty("players", new List<object> { playerInfo });

        // Act
        var result = _serializer.Serialize(playerInfoList);

        // Assert - Expected bytes: 01 00 ac 12 bd 00 00 00 80 3f 00 00 00 40 00 00 c0 3f 01 00 02 00 03 01 00 00 00 02 00 00 00
        var expectedBytes = new byte[] { 
            0x01, 0x00, 0xac, 0x12, 0xbd, 0x00, 0x00, 0x00, 0x80, 0x3f, 0x00, 0x00, 0x00, 0x40, 0x00, 0x00, 0xc0, 0x3f, 
            0x01, 0x00, 0x02, 0x00, 0x03, 0x01, 0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00 
        };
        Assert.Equal(expectedBytes, result);
    }

    [Fact]
    public void SerializeDeserialize_PlayerInfo_RoundTrip_ReturnsOriginalData()
    {
        // Arrange
        var playerInfoSchema = CreatePlayerInfoSchema();
        var playerInfoListSchema = CreatePlayerInfoListSchema();
        
        _serializer.RegisterSchema(playerInfoSchema);
        _serializer.RegisterSchema(playerInfoListSchema);

        var originalPlayerInfo = new DynamicGameDataObject(playerInfoSchema);
        originalPlayerInfo.SetProperty("id", 0x123456u);
        originalPlayerInfo.SetProperty("position", new Point3F(10.5f, 20.7f, 30.9f));
        originalPlayerInfo.SetProperty("crc", (ushort)100);
        originalPlayerInfo.SetProperty("customCrc", (ushort)200);
        originalPlayerInfo.SetProperty("dir", (byte)5);
        originalPlayerInfo.SetProperty("objectState1", 1000u);
        originalPlayerInfo.SetProperty("objectState2", 2000u);

        var originalPlayerInfoList = new DynamicGameDataObject(playerInfoListSchema);
        originalPlayerInfoList.SetProperty("players", new List<object> { originalPlayerInfo });

        // Act
        var serialized = _serializer.Serialize(originalPlayerInfoList);
        var deserialized = _serializer.Deserialize(serialized, playerInfoListSchema);

        // Assert
        var originalPlayers = originalPlayerInfoList.GetProperty<List<object>>("players");
        var deserializedPlayers = deserialized.GetProperty<List<object>>("players");
        
        Assert.Equal(originalPlayers.Count, deserializedPlayers.Count);
        
        var originalPlayer = originalPlayers[0] as DynamicGameDataObject;
        var deserializedPlayer = deserializedPlayers[0] as DynamicGameDataObject;
        
        Assert.Equal(originalPlayer.GetProperty<uint>("id"), deserializedPlayer.GetProperty<uint>("id"));
        
        var originalPos = originalPlayer.GetProperty<Point3F>("position");
        var deserializedPos = deserializedPlayer.GetProperty<Point3F>("position");
        Assert.NotNull(originalPos);
        Assert.NotNull(deserializedPos);
        Assert.Equal(originalPos.X, deserializedPos.X);
        Assert.Equal(originalPos.Z, deserializedPos.Z);
        Assert.Equal(originalPos.Y, deserializedPos.Y);
        
        Assert.Equal(originalPlayer.GetProperty<ushort>("crc"), deserializedPlayer.GetProperty<ushort>("crc"));
        Assert.Equal(originalPlayer.GetProperty<ushort>("customCrc"), deserializedPlayer.GetProperty<ushort>("customCrc"));
        Assert.Equal(originalPlayer.GetProperty<byte>("dir"), deserializedPlayer.GetProperty<byte>("dir"));
        Assert.Equal(originalPlayer.GetProperty<uint>("objectState1"), deserializedPlayer.GetProperty<uint>("objectState1"));
        Assert.Equal(originalPlayer.GetProperty<uint>("objectState2"), deserializedPlayer.GetProperty<uint>("objectState2"));
    }

    [Fact]
    public void SerializeDeserialize_Point3F_HandlesCorrectly()
    {
        // Arrange
        var schema = new GameDataSchema
        {
            Name = "Point3FTest",
            Type = "object",
            Properties = new Dictionary<string, PropertyDefinition>
            {
                ["position"] = new PropertyDefinition { Type = "point3f" }
            }
        };

        var gameDataObject = new DynamicGameDataObject(schema);
        gameDataObject.SetProperty("position", new Point3F(1.5f, 2.5f, 3.5f));

        // Act
        var serialized = _serializer.Serialize(gameDataObject);
        var deserialized = _serializer.Deserialize(serialized, schema);

        // Assert
        var position = deserialized.GetProperty<Point3F>("position");
        Assert.NotNull(position);
        Assert.Equal(1.5f, position.X);
        Assert.Equal(2.5f, position.Z);
        Assert.Equal(3.5f, position.Y);
    }

    [Fact]
    public void SerializeDeserialize_Point3F_WithDictionary_HandlesCorrectly()
    {
        // Arrange
        var schema = new GameDataSchema
        {
            Name = "Point3FTest",
            Type = "object",
            Properties = new Dictionary<string, PropertyDefinition>
            {
                ["position"] = new PropertyDefinition { Type = "point3f" }
            }
        };

        var gameDataObject = new DynamicGameDataObject(schema);
        var positionDict = new Dictionary<string, object>
        {
            ["x"] = 1.0f,
            ["z"] = 2.0f,
            ["y"] = 1.5f
        };
        gameDataObject.SetProperty("position", positionDict);

        // Act
        var serialized = _serializer.Serialize(gameDataObject);
        var deserialized = _serializer.Deserialize(serialized, schema);

        // Assert
        var position = deserialized.GetProperty<Point3F>("position");
        Assert.NotNull(position);
        Assert.Equal(1.0f, position.X);
        Assert.Equal(2.0f, position.Z);
        Assert.Equal(1.5f, position.Y);
    }

    [Fact]
    public void GetExpectedByteLength_PlayerInfoSchema_ReturnsCorrectLength()
    {
        // Arrange
        var playerInfoSchema = CreatePlayerInfoSchema();

        // Act
        var result = _serializer.GetExpectedByteLength(playerInfoSchema);

        // Assert - id(4) + position(12) + crc(2) + customCrc(2) + dir(1) + objectState1(4) + objectState2(4) = 29 bytes
        Assert.Equal(29, result);
    }

    [Fact]
    public void GetExpectedByteLength_Point3FSchema_ReturnsCorrectLength()
    {
        // Arrange
        var schema = new GameDataSchema
        {
            Name = "Point3FTest",
            Type = "object",
            Properties = new Dictionary<string, PropertyDefinition>
            {
                ["position"] = new PropertyDefinition { Type = "point3f" }
            }
        };

        // Act
        var result = _serializer.GetExpectedByteLength(schema);

        // Assert - 3 floats = 12 bytes
        Assert.Equal(12, result);
    }

    /// <summary>
    /// Creates the PlayerInfo schema for testing
    /// </summary>
    private GameDataSchema CreatePlayerInfoSchema()
    {
        return new GameDataSchema
        {
            Name = "PlayerInfo",
            Type = "object",
            Properties = new Dictionary<string, PropertyDefinition>
            {
                ["id"] = new PropertyDefinition { Type = "uint" },
                ["position"] = new PropertyDefinition { Type = "point3f" },
                ["crc"] = new PropertyDefinition { Type = "ushort" },
                ["customCrc"] = new PropertyDefinition { Type = "ushort" },
                ["dir"] = new PropertyDefinition { Type = "byte" },
                ["objectState1"] = new PropertyDefinition { Type = "uint" },
                ["objectState2"] = new PropertyDefinition { Type = "uint" }
            }
        };
    }

    /// <summary>
    /// Creates the PlayerInfo1List schema for testing
    /// </summary>
    private GameDataSchema CreatePlayerInfoListSchema()
    {
        return new GameDataSchema
        {
            Name = "PlayerInfo1List",
            Type = "object",
            Properties = new Dictionary<string, PropertyDefinition>
            {
                ["players"] = new PropertyDefinition 
                { 
                    Type = "array",
                    Meta = new Dictionary<string, object>
                    {
                        ["size"] = 2,
                        ["itemType"] = "ref(playerinfo.0x00.yml)"
                    }
                }
            }
        };
    }
}