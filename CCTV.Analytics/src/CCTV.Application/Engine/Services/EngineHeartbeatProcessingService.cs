using System.Text.Json;
using CCTV.Application.Common.Interfaces;
using CCTV.Application.Engine.Interfaces;
using CCTV.Application.Orchestration.Interfaces;
using CCTV.Application.Orchestration.Services;
using CCTV.Contracts.Engine;
using CCTV.Domain.Entities.Orchestration;
using CCTV.Domain.Enumerations;

namespace CCTV.Application.Engine.Services;

public class EngineHeartbeatProcessingService : IEngineHeartbeatProcessingService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEngineHealthEvaluationService _healthEvaluationService;
    private readonly IEngineTelemetryPublisher? _telemetryPublisher;

    public EngineHeartbeatProcessingService(
        IUnitOfWork unitOfWork,
        IEngineTelemetryPublisher? telemetryPublisher)
        : this(unitOfWork, null, telemetryPublisher)
    {
    }

    public EngineHeartbeatProcessingService(
        IUnitOfWork unitOfWork,
        IEngineHealthEvaluationService? healthEvaluationService = null,
        IEngineTelemetryPublisher? telemetryPublisher = null)
    {
        _unitOfWork = unitOfWork;
        _healthEvaluationService = healthEvaluationService ?? new EngineHealthEvaluationService();
        _telemetryPublisher = telemetryPublisher;
    }

    public async Task ProcessHeartbeatAsync(EngineHeartbeatMessage message, CancellationToken cancellationToken = default)
    {
        var instanceRepo = _unitOfWork.Repository<EngineInstance>();
        var heartbeatRepo = _unitOfWork.Repository<EngineHeartbeat>();
        var healthHistoryRepo = _unitOfWork.Repository<EngineHealthHistory>();

        var instanceIdStr = !string.IsNullOrWhiteSpace(message.InstanceId) ? message.InstanceId : message.EngineId.ToString();

        var instance = await instanceRepo.FirstOrDefaultAsync(
            i => (i.InstanceId == instanceIdStr || i.EngineId == message.EngineId) && i.ApplicationId == message.ApplicationId,
            cancellationToken);

        if (instance == null)
        {
            return;
        }

        var vramUsed = message.SystemMetrics?.GpuMetrics?.VramUsedMb > 0
            ? message.SystemMetrics.GpuMetrics.VramUsedMb
            : message.GpuMemoryUsedMb;

        var vramTotal = message.SystemMetrics?.GpuMetrics?.VramTotalMb > 0
            ? message.SystemMetrics.GpuMetrics.VramTotalMb
            : instance.TotalVramMb;

        var cpuPct = message.SystemMetrics != null && message.SystemMetrics.CpuUsagePercent > 0
            ? message.SystemMetrics.CpuUsagePercent
            : (float)message.CpuUsagePercent;

        var gpuPct = message.SystemMetrics?.GpuMetrics != null && message.SystemMetrics.GpuMetrics.GpuUsagePercent > 0
            ? message.SystemMetrics.GpuMetrics.GpuUsagePercent
            : (float)message.GpuUsagePercent;

        var gpuTemp = message.SystemMetrics?.GpuMetrics?.GpuTemperatureC ?? 0.0f;
        var activeStreams = message.Pipelines?.Count ?? 0;
        var avgFps = message.Pipelines != null && message.Pipelines.Any() ? (float)message.Pipelines.Average(p => p.Fps) : 0.0f;
        var totalDropped = message.Pipelines != null && message.Pipelines.Any() ? (int)message.Pipelines.Sum(p => p.DroppedFrames) : 0;

        var msgTimestampUtc = message.TimestampUtc != default ? message.TimestampUtc : DateTime.UtcNow;
        var elapsedSec = (DateTime.UtcNow - msgTimestampUtc).TotalSeconds;

        // Evaluate Health Score & State
        var evalResult = _healthEvaluationService.EvaluateHealth(
            cpuPct, gpuPct, vramUsed, vramTotal, gpuTemp, totalDropped, elapsedSec);

        // Sequence Validation Guard (Ignore Out-of-Order timestamps for LastHeartbeatUtc)
        if (msgTimestampUtc >= instance.LastHeartbeatUtc)
        {
            instance.LastHeartbeatUtc = msgTimestampUtc;
            instance.State = evalResult.HealthState == HealthStatus.Healthy ? EngineState.Online : EngineState.Degraded;
            instance.Status = instance.State.ToString();
            instance.AllocatedGpuMemoryMb = vramUsed;
            instance.ActivePipelinesCount = activeStreams;
            instanceRepo.Update(instance);
        }

        var heartbeatLog = new EngineHeartbeat
        {
            ApplicationId = message.ApplicationId,
            InstanceId = instance.Id,
            InstanceIdString = instanceIdStr,
            SequenceNumber = message.SequenceNumber,
            Timestamp = message.Timestamp,
            TimestampUtc = msgTimestampUtc,
            CpuUsagePercent = cpuPct,
            GpuUsagePercent = gpuPct,
            VramUsedMb = vramUsed,
            GpuTemperatureC = gpuTemp,
            ActivePipelinesCount = activeStreams,
            TelemetryJson = JsonSerializer.Serialize(message.Pipelines ?? new List<PipelineTelemetryDto>()),
            CpuUsagePercentage = (decimal)cpuPct,
            GpuUsagePercentage = (decimal)gpuPct,
            GpuMemoryUsedMb = vramUsed,
            InferenceFps = (decimal)avgFps,
            FrameDropCount = totalDropped,
            ActiveStreamCount = activeStreams
        };

        await heartbeatRepo.AddAsync(heartbeatLog, cancellationToken);

        var healthHistoryLog = new EngineHealthHistory
        {
            ApplicationId = message.ApplicationId,
            EngineInstanceId = instance.Id,
            CpuUsagePercent = cpuPct,
            GpuUsagePercent = gpuPct,
            VramUsedMb = vramUsed,
            TotalVramMb = vramTotal,
            GpuTemperatureC = gpuTemp,
            InferenceFps = avgFps,
            FrameDropCount = totalDropped,
            ActivePipelinesCount = activeStreams,
            HealthScore = evalResult.HealthScore,
            HealthState = evalResult.HealthState,
            TimestampUtc = msgTimestampUtc
        };

        if (healthHistoryRepo != null)
        {
            await healthHistoryRepo.AddAsync(healthHistoryLog, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Broadcast live telemetry update via SignalR / telemetry publisher
        if (_telemetryPublisher != null)
        {
            var telemetryBroadcasting = new EngineTelemetryBroadcastingDto(
                InstanceId: instanceIdStr,
                CpuUsagePercent: cpuPct,
                GpuUsagePercent: gpuPct,
                VramUsedMb: vramUsed,
                VramTotalMb: vramTotal,
                ActivePipelines: activeStreams,
                TimestampUtc: instance.LastHeartbeatUtc
            );

            await _telemetryPublisher.PublishTelemetryAsync(telemetryBroadcasting, cancellationToken);
        }
    }

    public async Task EvaluateNodeTimeoutsAsync(CancellationToken cancellationToken = default)
    {
        var instanceRepo = _unitOfWork.Repository<EngineInstance>();
        var activeInstances = await instanceRepo.FindAsync(
            i => i.State == EngineState.Online || i.State == EngineState.Degraded,
            cancellationToken);

        var now = DateTime.UtcNow;
        var updated = false;

        foreach (var instance in activeInstances)
        {
            var elapsedSec = (now - instance.LastHeartbeatUtc).TotalSeconds;

            if (elapsedSec >= 30.0)
            {
                instance.MarkOffline();
                instanceRepo.Update(instance);
                updated = true;

                if (_telemetryPublisher != null)
                {
                    await _telemetryPublisher.PublishNodeStatusAsync(instance.InstanceId, "OFFLINE", cancellationToken);
                }
            }
            else if (elapsedSec >= 15.0 && instance.State != EngineState.Degraded)
            {
                instance.MarkDegraded();
                instanceRepo.Update(instance);
                updated = true;

                if (_telemetryPublisher != null)
                {
                    await _telemetryPublisher.PublishNodeStatusAsync(instance.InstanceId, "DEGRADED", cancellationToken);
                }
            }
        }

        if (updated)
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
