using BCAS.Api.Model;
using BCAS.Api.Repository.Interfaces;
using BCAS.Api.Repository.Utilities;
using Dapper;

namespace BCAS.Api.Repository;

public class UserRepository : IUserRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public UserRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        const string sql = @"
            SELECT u.Id, u.FullName, u.Email, u.PasswordHash, u.PasswordSalt,
                   u.RoleId, r.Name AS RoleName, u.DepartmentId, d.Name AS DepartmentName,
                   u.IsActive, u.CreatedAt, u.UpdatedAt
            FROM Users u
            INNER JOIN Roles r ON r.Id = u.RoleId
            LEFT JOIN Departments d ON d.Id = u.DepartmentId
            WHERE u.Email = @Email;";

        using var connection = _connectionFactory.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<User>(sql, new { Email = email });
    }

    public async Task<User?> GetByIdAsync(int id)
    {
        const string sql = @"
            SELECT u.Id, u.FullName, u.Email, u.PasswordHash, u.PasswordSalt,
                   u.RoleId, r.Name AS RoleName, u.DepartmentId, d.Name AS DepartmentName,
                   u.IsActive, u.CreatedAt, u.UpdatedAt
            FROM Users u
            INNER JOIN Roles r ON r.Id = u.RoleId
            LEFT JOIN Departments d ON d.Id = u.DepartmentId
            WHERE u.Id = @Id;";

        using var connection = _connectionFactory.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<User>(sql, new { Id = id });
    }

    public async Task RevokeTokenAsync(string jti, DateTime expiresAtUtc)
    {
        const string sql = @"
            INSERT INTO RevokedTokens (Jti, ExpiresAtUtc, RevokedAtUtc)
            VALUES (@Jti, @ExpiresAtUtc, SYSUTCDATETIME());";

        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(sql, new { Jti = jti, ExpiresAtUtc = expiresAtUtc });
    }

    public async Task<bool> IsTokenRevokedAsync(string jti)
    {
        const string sql = "SELECT COUNT(1) FROM RevokedTokens WHERE Jti = @Jti;";

        using var connection = _connectionFactory.CreateConnection();
        var count = await connection.ExecuteScalarAsync<int>(sql, new { Jti = jti });
        return count > 0;
    }
}
