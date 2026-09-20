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

    public async Task<(LoginResult Result, LoginResponseDto? Response)> LoginAsync(LoginRequestDto request)
    {
        var user = await _userRepository.GetByEmailOrUsernameAsync(request.EmailOrUsername);

        // Unknown account or wrong password: identical generic failure either
        // way, so guessing usernames can't be used to enumerate accounts.
        if (user is null || !PasswordHasher.VerifyPassword(request.Password, user.PasswordHash, user.PasswordSalt))
        {
            return (LoginResult.InvalidCredentials, null);
        }

        // Only reachable once the password is already proven correct, so this
        // doesn't leak deactivation status to someone who doesn't know it.
        if (!user.IsActive)
        {
            return (LoginResult.AccountDeactivated, null);
        }

        var (token, _, expiresAtUtc) = _tokenGenerator.GenerateToken(user);

        var response = new LoginResponseDto
        {
            Token = token,
            ExpiresAtUtc = expiresAtUtc,
            User = MapToDto(user),
        };
        return (LoginResult.Success, response);
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
        FirstName = user.FirstName,
        LastName = user.LastName,
        Username = user.Username,
        Email = user.Email,
        Role = user.RoleName,
        DepartmentId = user.DepartmentId,
        DepartmentName = user.DepartmentName,
    };
}
