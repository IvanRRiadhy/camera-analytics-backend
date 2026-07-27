using CCTV.Contracts.Common;

namespace CCTV.Contracts.Incident;

public record IncidentManagementContract : BaseMessage
{
    public Guid IncidentId { get; init; }
    public string CaseNumber { get; init; } = string.Empty;
    public Guid? AssignedUserId { get; init; }
    public string Priority { get; init; } = "High";
    public string Status { get; init; } = "Open";
    public string Title { get; init; } = string.Empty;
    public string? ResolutionNotes { get; init; }
}
