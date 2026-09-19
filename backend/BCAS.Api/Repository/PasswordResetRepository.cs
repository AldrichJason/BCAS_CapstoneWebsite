using BCAS.Api.Repository.Interfaces;
using BCAS.Api.Repository.Utilities;
using Dapper;

namespace BCAS.Api.Repository;

public class PasswordResetRepository : IPasswordResetRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public PasswordResetRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task CreateTokenAsync(int userId, string tokenHash, DateTime expiresAtUtc)
    {
        const string sql = @"
            INSERT INTO PasswordResetTokens (UserId, TokenHash, ExpiresAtUtc)
            VALUES (@UserId, @TokenHash, @ExpiresAtUtc);";

        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(sql, new { UserId = userId, TokenHash = tokenHash, ExpiresAtUtc = expiresAtUtc });
    }

    public async Task<ValidResetToken?> GetValidTokenAsync(string tokenHash)
    {
        const string sql = @"
            SELECT Id, UserId
            FROM PasswordResetTokens
            WHERE TokenHash = @TokenHash
              AND UsedAtUtc IS NULL
              AND ExpiresAtUtc > SYSUTCDATETIME();";

        using var connection = _connectionFactory.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<ValidResetToken>(sql, new { TokenHash = tokenHash });
    }

    public async Task MarkUsedAsync(int tokenId)
    {
        const string sql = "UPDATE PasswordResetTokens SET UsedAtUtc = SYSUTCDATETIME() WHERE Id = @Id;";

        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(sql, new { Id = tokenId });
    }

    public async Task InvalidateOutstandingTokensAsync(int userId)
    {
        const string sql = @"
            UPDATE PasswordResetTokens
            SET UsedAtUtc = SYSUTCDATETIME()
            WHERE UserId = @UserId AND UsedAtUtc IS NULL;";

        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(sql, new { UserId = userId });
    }
}
