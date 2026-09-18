using System.Data;

namespace BCAS.Api.Repository.Utilities;

public interface IDbConnectionFactory
{
    IDbConnection CreateConnection();
}
