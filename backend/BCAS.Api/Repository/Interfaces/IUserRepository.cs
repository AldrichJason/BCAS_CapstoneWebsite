using BCAS.Api.Model;

namespace BCAS.Api.Repository.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByIdAsync(int id);
    Task RevokeTokenAsync(string jti, DateTime expiresAtUtc);
    Task<bool> IsTokenRevokedAsync(string jti);
}
