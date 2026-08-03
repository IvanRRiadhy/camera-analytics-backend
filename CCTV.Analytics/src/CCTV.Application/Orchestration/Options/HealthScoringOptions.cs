namespace CCTV.Application.Orchestration.Options;

public class HealthScoringOptions
{
    public const string SectionName = "HealthScoring";

    public float CpuThresholdPercent { get; set; } = 90.0f;
    public float GpuThresholdPercent { get; set; } = 90.0f;
    public float VramThresholdPercent { get; set; } = 90.0f;
    public float GpuTempThresholdC { get; set; } = 80.0f;
    public int FrameDropThreshold { get; set; } = 10;
    public double MissedHeartbeatWarningSec { get; set; } = 15.0;
    public double MissedHeartbeatOfflineSec { get; set; } = 30.0;
}
