using BCAS.Api.Model.DTOs;

namespace BCAS.Api.Service.Interfaces;

public interface IAuthService
{
    Task<LoginResponseDto?> LoginAsync(LoginRequestDto request);
    Task<UserDto?> GetActiveUserAsync(int userId);
    Task LogoutAsync(string jti, DateTime expiresAtUtc);
}
