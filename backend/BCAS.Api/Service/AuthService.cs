using BCAS.Api.Helper;
using BCAS.Api.Model.DTOs;
using BCAS.Api.Repository.Interfaces;
using BCAS.Api.Service.Interfaces;

namespace BCAS.Api.Service;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly JwtTokenGenerator _tokenGenerator;

    public AuthService(IUserRepository userRepository, JwtTokenGenerator tokenGenerator)
    {
        _userRepository = userRepository;
        _tokenGenerator = tokenGenerator;
    }

    public async Task<LoginResponseDto?> LoginAsync(LoginRequestDto request)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);

        // Generic failure for unknown email, inactive account or bad password:
        // never reveal which condition failed (BW-10 AC).
        if (user is null || !user.IsActive)
        {
            return null;
        }

        if (!PasswordHasher.VerifyPassword(request.Password, user.PasswordHash, user.PasswordSalt))
        {
            return null;
        }

        var (token, _, expiresAtUtc) = _tokenGenerator.GenerateToken(user);

        return new LoginResponseDto
        {
            Token = token,
            ExpiresAtUtc = expiresAtUtc,
            User = MapToDto(user),
        };
    }

    public async Task<UserDto?> GetActiveUserAsync(int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user is null || !user.IsActive)
        {
            return null;
        }

        return MapToDto(user);
    }

    public Task LogoutAsync(string jti, DateTime expiresAtUtc) =>
        _userRepository.RevokeTokenAsync(jti, expiresAtUtc);

    private static UserDto MapToDto(Model.User user) => new()
    {
        Id = user.Id,
        FullName = user.FullName,
        Email = user.Email,
        Role = user.RoleName,
        DepartmentId = user.DepartmentId,
        DepartmentName = user.DepartmentName,
    };
}
