namespace CCTV.Shared.Interfaces;

public interface ICurrentUserContext
{
    Guid? UserId { get; }
    string? UserEmail { get; }
    bool IsAuthenticated { get; }
}
