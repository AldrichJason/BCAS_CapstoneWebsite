using BCAS.Api.Model;
using BCAS.Api.Repository.Interfaces;
using BCAS.Api.Repository.Utilities;
using Dapper;

namespace BCAS.Api.Repository;

public class EventRepository : IEventRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    private const string SelectColumns = @"
        e.Id, e.Title, e.Body AS Description, e.EventStartUtc, e.EventEndUtc, e.Location AS Venue,
        e.DepartmentId, d.Name AS DepartmentName, e.Status,
        e.CreatedBy, (creator.FirstName + ' ' + creator.LastName) AS CreatedByName, e.CreatedAt,
        e.UpdatedBy, (updater.FirstName + ' ' + updater.LastName) AS UpdatedByName, e.UpdatedAt
        FROM Events e
        LEFT JOIN Departments d ON d.Id = e.DepartmentId
        INNER JOIN Users creator ON creator.Id = e.CreatedBy
        LEFT JOIN Users updater ON updater.Id = e.UpdatedBy";

    public EventRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<Event?> GetByIdAsync(int id)
    {
        var sql = $"SELECT {SelectColumns} WHERE e.Id = @Id;";

        using var connection = _connectionFactory.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<Event>(sql, new { Id = id });
    }

    public async Task<IReadOnlyList<Event>> GetByDepartmentAsync(int departmentId, string? status)
    {
        var sql = $@"SELECT {SelectColumns}
            WHERE e.DepartmentId = @DepartmentId
              AND (@Status IS NULL OR e.Status = @Status)
            ORDER BY e.EventStartUtc DESC;";

        using var connection = _connectionFactory.CreateConnection();
        var rows = await connection.QueryAsync<Event>(sql, new { DepartmentId = departmentId, Status = status });
        return rows.AsList();
    }

    public async Task<IReadOnlyList<Event>> GetAllAsync(int? departmentId, string? status)
    {
        var sql = $@"SELECT {SelectColumns}
            WHERE (
                    @DepartmentId IS NULL
                 OR (@DepartmentId = 0 AND e.DepartmentId IS NULL)
                 OR (@DepartmentId > 0 AND e.DepartmentId = @DepartmentId)
                  )
              AND (@Status IS NULL OR e.Status = @Status)
            ORDER BY e.EventStartUtc DESC;";

        using var connection = _connectionFactory.CreateConnection();
        var rows = await connection.QueryAsync<Event>(sql, new { DepartmentId = departmentId, Status = status });
        return rows.AsList();
    }

    public async Task<int> CreateAsync(Event evt)
    {
        const string sql = @"
            INSERT INTO Events (Title, Body, EventStartUtc, EventEndUtc, Location, DepartmentId, Status, CreatedBy, CreatedAt)
            OUTPUT INSERTED.Id
            VALUES (@Title, @Description, @EventStartUtc, @EventEndUtc, @Venue, @DepartmentId, @Status, @CreatedBy, SYSUTCDATETIME());";

        using var connection = _connectionFactory.CreateConnection();
        return await connection.ExecuteScalarAsync<int>(sql, new
        {
            evt.Title,
            evt.Description,
            evt.EventStartUtc,
            evt.EventEndUtc,
            evt.Venue,
            evt.DepartmentId,
            evt.Status,
            evt.CreatedBy,
        });
    }

    public async Task<bool> UpdateAsync(Event evt)
    {
        const string sql = @"
            UPDATE Events
            SET Title = @Title,
                Body = @Description,
                EventStartUtc = @EventStartUtc,
                EventEndUtc = @EventEndUtc,
                Location = @Venue,
                Status = @Status,
                UpdatedBy = @UpdatedBy,
                UpdatedAt = SYSUTCDATETIME()
            WHERE Id = @Id;";

        using var connection = _connectionFactory.CreateConnection();
        var rows = await connection.ExecuteAsync(sql, new
        {
            evt.Id,
            evt.Title,
            evt.Description,
            evt.EventStartUtc,
            evt.EventEndUtc,
            evt.Venue,
            evt.Status,
            evt.UpdatedBy,
        });
        return rows > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        const string sql = "DELETE FROM Events WHERE Id = @Id;";

        using var connection = _connectionFactory.CreateConnection();
        var rows = await connection.ExecuteAsync(sql, new { Id = id });
        return rows > 0;
    }
}
