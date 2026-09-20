using BCAS.Api.Model;
using BCAS.Api.Repository.Interfaces;
using BCAS.Api.Repository.Utilities;
using Dapper;

namespace BCAS.Api.Repository;

public class NewsRepository : INewsRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    private const string SelectColumns = @"
        n.Id, n.Title, n.Body, n.DepartmentId, d.Name AS DepartmentName, n.Status, n.PublishAtUtc,
        n.CreatedBy, creator.FullName AS CreatedByName, n.CreatedAt,
        n.UpdatedBy, updater.FullName AS UpdatedByName, n.UpdatedAt
        FROM News n
        LEFT JOIN Departments d ON d.Id = n.DepartmentId
        INNER JOIN Users creator ON creator.Id = n.CreatedBy
        LEFT JOIN Users updater ON updater.Id = n.UpdatedBy";

    public NewsRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<News?> GetByIdAsync(int id)
    {
        var sql = $"SELECT {SelectColumns} WHERE n.Id = @Id;";

        using var connection = _connectionFactory.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<News>(sql, new { Id = id });
    }

    public async Task<IReadOnlyList<News>> GetByDepartmentAsync(int departmentId, string? status)
    {
        var sql = $@"SELECT {SelectColumns}
            WHERE n.DepartmentId = @DepartmentId
              AND (@Status IS NULL OR n.Status = @Status)
            ORDER BY n.UpdatedAt DESC, n.CreatedAt DESC;";

        using var connection = _connectionFactory.CreateConnection();
        var rows = await connection.QueryAsync<News>(sql, new { DepartmentId = departmentId, Status = status });
        return rows.AsList();
    }

    public async Task<int> CreateAsync(News news)
    {
        const string sql = @"
            INSERT INTO News (Title, Body, DepartmentId, Status, PublishAtUtc, CreatedBy, CreatedAt)
            OUTPUT INSERTED.Id
            VALUES (@Title, @Body, @DepartmentId, @Status, @PublishAtUtc, @CreatedBy, SYSUTCDATETIME());";

        using var connection = _connectionFactory.CreateConnection();
        return await connection.ExecuteScalarAsync<int>(sql, new
        {
            news.Title,
            news.Body,
            news.DepartmentId,
            news.Status,
            news.PublishAtUtc,
            news.CreatedBy,
        });
    }

    public async Task<bool> UpdateAsync(News news)
    {
        const string sql = @"
            UPDATE News
            SET Title = @Title,
                Body = @Body,
                Status = @Status,
                PublishAtUtc = @PublishAtUtc,
                UpdatedBy = @UpdatedBy,
                UpdatedAt = SYSUTCDATETIME()
            WHERE Id = @Id;";

        using var connection = _connectionFactory.CreateConnection();
        var rows = await connection.ExecuteAsync(sql, new
        {
            news.Id,
            news.Title,
            news.Body,
            news.Status,
            news.PublishAtUtc,
            news.UpdatedBy,
        });
        return rows > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        const string sql = "DELETE FROM News WHERE Id = @Id;";

        using var connection = _connectionFactory.CreateConnection();
        var rows = await connection.ExecuteAsync(sql, new { Id = id });
        return rows > 0;
    }
}
