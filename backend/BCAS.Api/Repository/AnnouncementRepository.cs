using BCAS.Api.Model;
using BCAS.Api.Repository.Interfaces;
using BCAS.Api.Repository.Utilities;
using Dapper;

namespace BCAS.Api.Repository;

public class AnnouncementRepository : IAnnouncementRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    private const string SelectColumns = @"
        a.Id, a.Title, a.Body, a.DepartmentId, d.Name AS DepartmentName, a.Status, a.PublishAtUtc,
        a.CreatedBy, creator.FullName AS CreatedByName, a.CreatedAt,
        a.UpdatedBy, updater.FullName AS UpdatedByName, a.UpdatedAt
        FROM Announcements a
        LEFT JOIN Departments d ON d.Id = a.DepartmentId
        INNER JOIN Users creator ON creator.Id = a.CreatedBy
        LEFT JOIN Users updater ON updater.Id = a.UpdatedBy";

    public AnnouncementRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<Announcement?> GetByIdAsync(int id)
    {
        var sql = $"SELECT {SelectColumns} WHERE a.Id = @Id;";

        using var connection = _connectionFactory.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<Announcement>(sql, new { Id = id });
    }

    public async Task<IReadOnlyList<Announcement>> GetByDepartmentAsync(int departmentId, string? status, bool sortDescending)
    {
        var direction = sortDescending ? "DESC" : "ASC";
        var sql = $@"SELECT {SelectColumns}
            WHERE a.DepartmentId = @DepartmentId
              AND (@Status IS NULL OR a.Status = @Status)
            ORDER BY a.PublishAtUtc {direction};";

        using var connection = _connectionFactory.CreateConnection();
        var rows = await connection.QueryAsync<Announcement>(sql, new { DepartmentId = departmentId, Status = status });
        return rows.AsList();
    }

    public async Task<int> CreateAsync(Announcement announcement)
    {
        const string sql = @"
            INSERT INTO Announcements (Title, Body, DepartmentId, Status, PublishAtUtc, CreatedBy, CreatedAt)
            OUTPUT INSERTED.Id
            VALUES (@Title, @Body, @DepartmentId, @Status, @PublishAtUtc, @CreatedBy, SYSUTCDATETIME());";

        using var connection = _connectionFactory.CreateConnection();
        return await connection.ExecuteScalarAsync<int>(sql, new
        {
            announcement.Title,
            announcement.Body,
            announcement.DepartmentId,
            announcement.Status,
            announcement.PublishAtUtc,
            announcement.CreatedBy,
        });
    }

    public async Task<bool> UpdateAsync(Announcement announcement)
    {
        const string sql = @"
            UPDATE Announcements
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
            announcement.Id,
            announcement.Title,
            announcement.Body,
            announcement.Status,
            announcement.PublishAtUtc,
            announcement.UpdatedBy,
        });
        return rows > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        const string sql = "DELETE FROM Announcements WHERE Id = @Id;";

        using var connection = _connectionFactory.CreateConnection();
        var rows = await connection.ExecuteAsync(sql, new { Id = id });
        return rows > 0;
    }
}
