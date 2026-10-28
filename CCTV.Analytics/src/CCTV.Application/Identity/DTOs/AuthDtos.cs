namespace CCTV.Application.Identity.DTOs;

public record LoginRequestDto(string Username, string Password, Guid? TenantId = null);

public record LoginResponseDto(string AccessToken, string TokenType, int ExpiresIn, UserDto User);

public record CurrentUserProfileDto(Guid Id, string Username, string Email, string FullName, List<string> Roles, List<string>? Permissions = null);
