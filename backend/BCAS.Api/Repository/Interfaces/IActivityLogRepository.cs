namespace BCAS.Api.Repository.Interfaces;

public interface IActivityLogRepository
{
    Task LogAsync(int? userId, string action, string? entityType, int? entityId, string? details);
}
