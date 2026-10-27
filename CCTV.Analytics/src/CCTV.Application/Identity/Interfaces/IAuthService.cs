using CCTV.Application.Identity.DTOs;

namespace CCTV.Application.Identity.Interfaces;

public interface IAuthService
{
    Task<LoginResponseDto> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default);
    Task<CurrentUserProfileDto?> GetCurrentUserAsync(CancellationToken cancellationToken = default);
}
