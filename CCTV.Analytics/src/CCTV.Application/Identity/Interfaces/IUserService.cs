using CCTV.Application.Identity.DTOs;

namespace CCTV.Application.Identity.Interfaces;

public interface IUserService
{
    Task<IReadOnlyList<UserDto>> GetUsersAsync(CancellationToken cancellationToken = default);
    Task<UserDto?> GetUserByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<UserDto> CreateUserAsync(CreateUserRequestDto request, CancellationToken cancellationToken = default);
    Task<UserDto> UpdateUserAsync(Guid id, UpdateUserRequestDto request, CancellationToken cancellationToken = default);
    Task<bool> DeleteUserAsync(Guid id, CancellationToken cancellationToken = default);
}
