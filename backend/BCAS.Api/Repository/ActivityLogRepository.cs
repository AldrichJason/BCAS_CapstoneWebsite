using BCAS.Api.Repository.Interfaces;
using BCAS.Api.Repository.Utilities;
using Dapper;

namespace BCAS.Api.Repository;

public class ActivityLogRepository : IActivityLogRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public ActivityLogRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task LogAsync(int? userId, string action, string? entityType, int? entityId, string? details)
    {
        const string sql = @"
            INSERT INTO ActivityLog (UserId, Action, EntityType, EntityId, Details, CreatedAt)
            VALUES (@UserId, @Action, @EntityType, @EntityId, @Details, SYSUTCDATETIME());";

        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(sql, new { UserId = userId, Action = action, EntityType = entityType, EntityId = entityId, Details = details });
    }
}
