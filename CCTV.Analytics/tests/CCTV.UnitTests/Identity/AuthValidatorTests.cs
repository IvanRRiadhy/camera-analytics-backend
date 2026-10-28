using CCTV.Application.Identity.DTOs;
using CCTV.Application.Identity.Validators;
using Xunit;

namespace CCTV.UnitTests.Identity;

public class AuthValidatorTests
{
    private readonly LoginRequestValidator _loginValidator = new();
    private readonly CreateUserRequestValidator _createUserValidator = new();

    [Fact]
    public void LoginValidator_EmptyUsername_FailsValidation()
    {
        var result = _loginValidator.Validate(new LoginRequestDto(Username: "", Password: "password123"));
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Username");
    }

    [Fact]
    public void LoginValidator_EmptyPassword_FailsValidation()
    {
        var result = _loginValidator.Validate(new LoginRequestDto(Username: "admin", Password: ""));
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Password");
    }

    [Fact]
    public void LoginValidator_ValidUsernameAndPassword_PassesValidation()
    {
        var result = _loginValidator.Validate(new LoginRequestDto(Username: "admin", Password: "Secret123!"));
        Assert.True(result.IsValid);
    }

    [Fact]
    public void CreateUserValidator_EmptyUsername_FailsValidation()
    {
        var result = _createUserValidator.Validate(new CreateUserRequestDto("", "user@bionics.ai", "123456", "Test User", null, new List<string> { "Operator" }));
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Username");
    }

    [Fact]
    public void CreateUserValidator_PasswordLessThan6Chars_FailsValidation()
    {
        var result = _createUserValidator.Validate(new CreateUserRequestDto("user", "user@bionics.ai", "12345", "Test User", null, new List<string> { "Operator" }));
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Password");
    }

    [Fact]
    public void CreateUserValidator_Password6CharsOrMore_PassesValidation()
    {
        var result = _createUserValidator.Validate(new CreateUserRequestDto("user", "user@bionics.ai", "123456", "Test User", null, new List<string> { "Operator" }));
        Assert.True(result.IsValid);
    }
}
