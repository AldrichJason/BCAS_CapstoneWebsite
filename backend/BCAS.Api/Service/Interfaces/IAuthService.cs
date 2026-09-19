using BCAS.Api.Model.DTOs;

namespace BCAS.Api.Service.Interfaces;

public enum LoginResult
{
    Success,
    InvalidCredentials,
    AccountDeactivated,
}

public interface IAuthService
{
    /// <summary>
    /// AccountDeactivated is only ever returned once the password has already
    /// been verified — a wrong password on a deactivated account still gets
    /// the generic InvalidCredentials, so guessing usernames can't be used to
    /// discover which accounts exist or are deactivated.
    /// </summary>
    Task<(LoginResult Result, LoginResponseDto? Response)> LoginAsync(LoginRequestDto request);

    Task<UserDto?> GetActiveUserAsync(int userId);
    Task LogoutAsync(string jti, DateTime expiresAtUtc);
}
