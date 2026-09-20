namespace BCAS.Api.Repository.Interfaces;

public interface IActivityLogRepository
{
    Task LogAsync(int? userId, string action, string? entityType = null, int? entityId = null, string? details = null);
}
