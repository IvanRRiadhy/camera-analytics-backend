namespace CCTV.Application.Orchestration.Options;

public class MonitoringOptions
{
    public const string SectionName = "Monitoring";

    public bool DemoMode { get; set; } = true;
    public string DefaultRtspUrl { get; set; } = "rtsp://admin:Admin1234@192.168.1.64/Streaming/channels/2";
}
