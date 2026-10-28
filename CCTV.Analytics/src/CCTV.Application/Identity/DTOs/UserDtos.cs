namespace CCTV.Application.Identity.DTOs;

public record UserDto(
    Guid Id,
    string Username,
    string Email,
    string FullName,
    string? PhoneNumber,
    bool IsActive,
    DateTime? LastLoginAt,
    List<string> Roles);

public record CreateUserRequestDto(
    string Username,
    string Email,
    string Password,
    string FullName,
    string? PhoneNumber,
    List<string> Roles);

public record UpdateUserRequestDto(
    string FullName,
    string? PhoneNumber,
    bool IsActive,
    List<string>? Roles);
