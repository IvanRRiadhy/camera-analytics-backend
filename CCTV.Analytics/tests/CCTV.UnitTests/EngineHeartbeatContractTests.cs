using System.Text.Json;
using CCTV.Contracts.Engine;

namespace CCTV.UnitTests;

public class EngineHeartbeatContractTests
{
    [Fact]
    public void EngineHeartbeatMessage_Serialization_RoundTrip_Succeeds()
    {
        // Arrange
        var heartbeat = new EngineHeartbeatMessage
        {
            InstanceId = "engine_edge_node_01",
            TimestampUtc = DateTime.UtcNow,
            SequenceNumber = 1420,
            SystemMetrics = new SystemMetricsDto
            {
                CpuUsagePercent = 34.2f,
                MemoryUsedMb = 12450,
                MemoryTotalMb = 65536,
                GpuMetrics = new GpuMetricsDto
                {
                    GpuUsagePercent = 68.5f,
                    VramUsedMb = 14538,
                    VramTotalMb = 24576,
                    GpuTemperatureC = 62.0f,
                    PowerDrawWatts = 215.4f
                }
            },
            Pipelines = new List<PipelineTelemetryDto>
            {
                new PipelineTelemetryDto
                {
                    PipelineId = "pipeline_cam_lobby_01",
                    CameraId = "cam_lobby_01",
                    State = "RUNNING",
                    Fps = 10.1f,
                    FrameCount = 14200,
                    DroppedFrames = 0,
                    AvgFrameLatencyMs = 18.4f,
                    RtspStatus = "CONNECTED"
                }
            },
            ServiceRuntime = new ServiceRuntimeTelemetryDto
            {
                Status = "HEALTHY",
                RunningServices = new List<string> { "recording", "alarm", "notification", "investigation" },
                ErrorCount = 0
            }
        };

        // Act
        var json = JsonSerializer.Serialize(heartbeat);
        var deserialized = JsonSerializer.Deserialize<EngineHeartbeatMessage>(json);

        // Assert
        Assert.NotNull(deserialized);
        Assert.Equal(heartbeat.InstanceId, deserialized.InstanceId);
        Assert.Equal(heartbeat.SequenceNumber, deserialized.SequenceNumber);
        Assert.Equal(68.5f, deserialized.SystemMetrics.GpuMetrics.GpuUsagePercent);
        Assert.Single(deserialized.Pipelines);
        Assert.Equal("cam_lobby_01", deserialized.Pipelines[0].CameraId);
        Assert.Equal("HEALTHY", deserialized.ServiceRuntime.Status);
    }
}
