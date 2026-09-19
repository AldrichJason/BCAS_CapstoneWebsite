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
            SELECT u.Id, u.FirstName, u.LastName, u.Username, u.Email, u.PasswordHash, u.PasswordSalt,
                   u.RoleId, r.Name AS RoleName, u.DepartmentId, d.Name AS DepartmentName,
                   u.IsActive, u.CreatedAt, u.UpdatedAt
            FROM Users u
            INNER JOIN Roles r ON r.Id = u.RoleId
            LEFT JOIN Departments d ON d.Id = u.DepartmentId
            WHERE u.Email = @Email;";

        using var connection = _connectionFactory.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<User>(sql, new { Email = email });
    }

    public async Task<User?> GetByEmailOrUsernameAsync(string identifier)
    {
        const string sql = @"
            SELECT u.Id, u.FirstName, u.LastName, u.Username, u.Email, u.PasswordHash, u.PasswordSalt,
                   u.RoleId, r.Name AS RoleName, u.DepartmentId, d.Name AS DepartmentName,
                   u.IsActive, u.CreatedAt, u.UpdatedAt
            FROM Users u
            INNER JOIN Roles r ON r.Id = u.RoleId
            LEFT JOIN Departments d ON d.Id = u.DepartmentId
            WHERE u.Email = @Identifier OR u.Username = @Identifier;";

        using var connection = _connectionFactory.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<User>(sql, new { Identifier = identifier });
    }

    public async Task<User?> GetByIdAsync(int id)
    {
        const string sql = @"
            SELECT u.Id, u.FirstName, u.LastName, u.Username, u.Email, u.PasswordHash, u.PasswordSalt,
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

    public async Task<bool> EmailExistsAsync(string email)
    {
        const string sql = "SELECT COUNT(1) FROM Users WHERE Email = @Email;";

        using var connection = _connectionFactory.CreateConnection();
        var count = await connection.ExecuteScalarAsync<int>(sql, new { Email = email });
        return count > 0;
    }

    public async Task<bool> UsernameExistsAsync(string username)
    {
        const string sql = "SELECT COUNT(1) FROM Users WHERE Username = @Username;";

        using var connection = _connectionFactory.CreateConnection();
        var count = await connection.ExecuteScalarAsync<int>(sql, new { Username = username });
        return count > 0;
    }

    public async Task<Role?> GetRoleByNameAsync(string name)
    {
        const string sql = "SELECT Id, Name FROM Roles WHERE Name = @Name;";

        using var connection = _connectionFactory.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<Role>(sql, new { Name = name });
    }

    public async Task<bool> DepartmentExistsAsync(int departmentId)
    {
        const string sql = "SELECT COUNT(1) FROM Departments WHERE Id = @DepartmentId;";

        using var connection = _connectionFactory.CreateConnection();
        var count = await connection.ExecuteScalarAsync<int>(sql, new { DepartmentId = departmentId });
        return count > 0;
    }

    public async Task<int> CreateUserAsync(NewUser newUser)
    {
        const string sql = @"
            INSERT INTO Users (FirstName, LastName, Username, Email, PasswordHash, PasswordSalt, RoleId, DepartmentId, IsActive)
            OUTPUT INSERTED.Id
            VALUES (@FirstName, @LastName, @Username, @Email, @PasswordHash, @PasswordSalt, @RoleId, @DepartmentId, 1);";

        using var connection = _connectionFactory.CreateConnection();
        return await connection.ExecuteScalarAsync<int>(sql, newUser);
    }

    public async Task UpdatePasswordAsync(int userId, string passwordHash, string passwordSalt)
    {
        const string sql = @"
            UPDATE Users
            SET PasswordHash = @PasswordHash, PasswordSalt = @PasswordSalt, UpdatedAt = SYSUTCDATETIME()
            WHERE Id = @UserId;";

        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(sql, new { UserId = userId, PasswordHash = passwordHash, PasswordSalt = passwordSalt });
    }

    public async Task SetActiveAsync(int userId, bool isActive)
    {
        const string sql = @"
            UPDATE Users
            SET IsActive = @IsActive, UpdatedAt = SYSUTCDATETIME()
            WHERE Id = @UserId;";

        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(sql, new { UserId = userId, IsActive = isActive });
    }

    public async Task<IReadOnlyList<User>> ListAsync()
    {
        const string sql = @"
            SELECT u.Id, u.FirstName, u.LastName, u.Username, u.Email, u.PasswordHash, u.PasswordSalt,
                   u.RoleId, r.Name AS RoleName, u.DepartmentId, d.Name AS DepartmentName,
                   u.IsActive, u.CreatedAt, u.UpdatedAt
            FROM Users u
            INNER JOIN Roles r ON r.Id = u.RoleId
            LEFT JOIN Departments d ON d.Id = u.DepartmentId
            ORDER BY u.FirstName, u.LastName;";

        using var connection = _connectionFactory.CreateConnection();
        var users = await connection.QueryAsync<User>(sql);
        return users.AsList();
    }
}
