using System.Text.Json;
using CCTV.Contracts.Engine;

namespace CCTV.UnitTests;

public class EngineCommandContractTests
{
    [Fact]
    public void EngineCommandMessage_Serialization_RoundTrip_Succeeds()
    {
        // Arrange
        var command = new EngineCommandMessage
        {
            CommandId = "cmd_test_001",
            TargetInstanceId = "engine_edge_node_01",
            CommandType = "START_PIPELINE",
            CameraId = "cam_lobby_01",
            PayloadJson = "{\"fps\": 10.0, \"resolution\": [1920, 1080]}",
            DispatchedAtUtc = DateTime.UtcNow
        };

        // Act
        var json = JsonSerializer.Serialize(command);
        var deserialized = JsonSerializer.Deserialize<EngineCommandMessage>(json);

        // Assert
        Assert.NotNull(deserialized);
        Assert.Equal(command.CommandId, deserialized.CommandId);
        Assert.Equal(command.TargetInstanceId, deserialized.TargetInstanceId);
        Assert.Equal("START_PIPELINE", deserialized.CommandType);
        Assert.Equal("cam_lobby_01", deserialized.CameraId);
    }

    [Fact]
    public void EngineCommandResultDto_Serialization_RoundTrip_Succeeds()
    {
        // Arrange
        var result = new EngineCommandResultDto
        {
            CommandId = "cmd_test_001",
            TargetInstanceId = "engine_edge_node_01",
            Status = "SUCCESS",
            Message = "Pipeline cam_lobby_01 started successfully",
            ExecutedAtUtc = DateTime.UtcNow
        };

        // Act
        var json = JsonSerializer.Serialize(result);
        var deserialized = JsonSerializer.Deserialize<EngineCommandResultDto>(json);

        // Assert
        Assert.NotNull(deserialized);
        Assert.Equal(result.CommandId, deserialized.CommandId);
        Assert.Equal("SUCCESS", deserialized.Status);
    }
}
