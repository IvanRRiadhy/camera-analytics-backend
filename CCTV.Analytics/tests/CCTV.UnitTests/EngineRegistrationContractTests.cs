using System.Text.Json;
using CCTV.Contracts.Engine;

namespace CCTV.UnitTests;

public class EngineRegistrationContractTests
{
    [Fact]
    public void EngineRegistrationRequest_Serialization_RoundTrip_Succeeds()
    {
        // Arrange
        var request = new EngineRegistrationRequest
        {
            InstanceId = "engine_edge_node_01",
            WorkerNodeId = Guid.NewGuid(),
            Hostname = "edge-ai-server-01",
            IpAddress = "192.168.1.120",
            OsPlatform = "Linux-5.15.0",
            PythonVersion = "3.11.8",
            EngineVersion = "12.6.0",
            GpuInfo = new GpuInfoDto
            {
                DeviceName = "NVIDIA GeForce RTX 4090",
                CudaVersion = "12.2",
                DriverVersion = "535.129.03",
                TotalVramMb = 24576,
                CudaCores = 16384
            },
            CpuInfo = new CpuInfoDto
            {
                Architecture = "x86_64",
                ModelName = "AMD Ryzen 9 7950X",
                TotalCores = 16,
                TotalThreads = 32,
                TotalRamMb = 65536
            },
            InstalledPlugins = new List<PluginMetadataDto>
            {
                new PluginMetadataDto
                {
                    Name = "people_counter",
                    Version = "1.0.0",
                    SdkVersion = "12.0",
                    Capabilities = new List<string> { "people.count" }
                }
            },
            RegisteredAtUtc = DateTime.UtcNow
        };

        // Act
        var json = JsonSerializer.Serialize(request);
        var deserialized = JsonSerializer.Deserialize<EngineRegistrationRequest>(json);

        // Assert
        Assert.NotNull(deserialized);
        Assert.Equal(request.InstanceId, deserialized.InstanceId);
        Assert.Equal(request.WorkerNodeId, deserialized.WorkerNodeId);
        Assert.Equal(request.GpuInfo.DeviceName, deserialized.GpuInfo.DeviceName);
        Assert.Equal(request.CpuInfo.TotalRamMb, deserialized.CpuInfo.TotalRamMb);
        Assert.Single(deserialized.InstalledPlugins);
        Assert.Equal("people_counter", deserialized.InstalledPlugins[0].Name);
    }
}
