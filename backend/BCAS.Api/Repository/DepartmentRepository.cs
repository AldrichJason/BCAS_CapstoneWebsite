using BCAS.Api.Model;
using BCAS.Api.Repository.Interfaces;
using BCAS.Api.Repository.Utilities;
using Dapper;

namespace BCAS.Api.Repository;

public class DepartmentRepository : IDepartmentRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public DepartmentRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IReadOnlyList<Department>> GetAllAsync()
    {
        const string sql = "SELECT Id, Name FROM Departments ORDER BY Name;";

        using var connection = _connectionFactory.CreateConnection();
        var rows = await connection.QueryAsync<Department>(sql);
        return rows.AsList();
    }
}
