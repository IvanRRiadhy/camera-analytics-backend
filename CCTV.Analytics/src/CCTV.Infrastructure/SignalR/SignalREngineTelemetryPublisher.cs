using CCTV.Application.Common.Interfaces;
using CCTV.Contracts.Engine;
using Microsoft.AspNetCore.SignalR;

namespace CCTV.Infrastructure.SignalR;

public class SignalREngineTelemetryPublisher : EngineTelemetryPublisher
{
    public SignalREngineTelemetryPublisher(IHubContext<EngineTelemetryHub, IEngineTelemetryHubClient> hubContext)
        : base(hubContext)
    {
    }
}
