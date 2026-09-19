using BCAS.Api.Model;

namespace BCAS.Api.Repository.Interfaces;

public record NewUser(string FirstName, string LastName, string Username, string Email, string PasswordHash, string PasswordSalt, int RoleId, int? DepartmentId);

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByEmailOrUsernameAsync(string identifier);
    Task<User?> GetByIdAsync(int id);
    Task RevokeTokenAsync(string jti, DateTime expiresAtUtc);
    Task<bool> IsTokenRevokedAsync(string jti);

    Task<bool> EmailExistsAsync(string email);
    Task<bool> UsernameExistsAsync(string username);
    Task<Role?> GetRoleByNameAsync(string name);
    Task<bool> DepartmentExistsAsync(int departmentId);
    Task<int> CreateUserAsync(NewUser newUser);
    Task UpdatePasswordAsync(int userId, string passwordHash, string passwordSalt);
    Task SetActiveAsync(int userId, bool isActive);
    Task<IReadOnlyList<User>> ListAsync();
}
