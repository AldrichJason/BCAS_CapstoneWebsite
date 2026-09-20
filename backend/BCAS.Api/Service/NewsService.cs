using System.Text.Json;
using BCAS.Api.Common;
using BCAS.Api.Model;
using BCAS.Api.Model.DTOs;
using BCAS.Api.Repository.Interfaces;
using BCAS.Api.Service.Interfaces;

namespace BCAS.Api.Service;

/// <summary>
/// Department-scoped News CRUD for Academic Heads (BW-16). Every write is recorded
/// to ActivityLog and cross-department access is refused before it reaches the repository.
/// </summary>
public class NewsService : INewsService
{
    private readonly INewsRepository _newsRepository;
    private readonly IActivityLogRepository _activityLog;

    public NewsService(INewsRepository newsRepository, IActivityLogRepository activityLog)
    {
        _newsRepository = newsRepository;
        _activityLog = activityLog;
    }

    public async Task<IReadOnlyList<NewsDto>> GetDepartmentNewsAsync(int departmentId, string? status)
    {
        var rows = await _newsRepository.GetByDepartmentAsync(departmentId, status);
        return rows.Select(MapToDto).ToList();
    }

    public async Task<ScopedResult<NewsDto>> GetByIdAsync(int id, int departmentId)
    {
        var news = await _newsRepository.GetByIdAsync(id);
        if (news is null)
        {
            return ScopedResult<NewsDto>.NotFound();
        }

        if (news.DepartmentId != departmentId)
        {
            return ScopedResult<NewsDto>.Forbidden();
        }

        return ScopedResult<NewsDto>.Ok(MapToDto(news));
    }

    public async Task<ScopedResult<NewsDto>> CreateAsync(int departmentId, int userId, NewsRequestDto request)
    {
        if (!ContentStatus.IsValid(request.Status))
        {
            return ScopedResult<NewsDto>.Invalid($"Status must be one of: {string.Join(", ", ContentStatus.All)}.");
        }

        var news = new News
        {
            Title = request.Title,
            Body = request.Body,
            DepartmentId = departmentId,
            Status = request.Status,
            PublishAtUtc = request.PublishAtUtc,
            CreatedBy = userId,
        };

        news.Id = await _newsRepository.CreateAsync(news);

        await _activityLog.LogAsync(userId, "NewsCreated", "News", news.Id,
            JsonSerializer.Serialize(new { news.Title, news.Status, DepartmentId = departmentId }));

        var created = await _newsRepository.GetByIdAsync(news.Id);
        return ScopedResult<NewsDto>.Ok(MapToDto(created!));
    }

    public async Task<ScopedResult<NewsDto>> UpdateAsync(int id, int departmentId, int userId, NewsRequestDto request)
    {
        var existing = await _newsRepository.GetByIdAsync(id);
        if (existing is null)
        {
            return ScopedResult<NewsDto>.NotFound();
        }

        if (existing.DepartmentId != departmentId)
        {
            return ScopedResult<NewsDto>.Forbidden();
        }

        if (!ContentStatus.IsValid(request.Status))
        {
            return ScopedResult<NewsDto>.Invalid($"Status must be one of: {string.Join(", ", ContentStatus.All)}.");
        }

        existing.Title = request.Title;
        existing.Body = request.Body;
        existing.Status = request.Status;
        existing.PublishAtUtc = request.PublishAtUtc;
        existing.UpdatedBy = userId;

        var updated = await _newsRepository.UpdateAsync(existing);
        if (!updated)
        {
            return ScopedResult<NewsDto>.NotFound();
        }

        await _activityLog.LogAsync(userId, "NewsUpdated", "News", id,
            JsonSerializer.Serialize(new { existing.Title, existing.Status }));

        var refreshed = await _newsRepository.GetByIdAsync(id);
        return ScopedResult<NewsDto>.Ok(MapToDto(refreshed!));
    }

    public async Task<ScopedResult<bool>> DeleteAsync(int id, int departmentId, int userId)
    {
        var existing = await _newsRepository.GetByIdAsync(id);
        if (existing is null)
        {
            return ScopedResult<bool>.NotFound();
        }

        if (existing.DepartmentId != departmentId)
        {
            return ScopedResult<bool>.Forbidden();
        }

        var deleted = await _newsRepository.DeleteAsync(id);
        if (!deleted)
        {
            return ScopedResult<bool>.NotFound();
        }

        await _activityLog.LogAsync(userId, "NewsDeleted", "News", id,
            JsonSerializer.Serialize(new { existing.Title }));

        return ScopedResult<bool>.Ok(true);
    }

    public async Task<IReadOnlyList<NewsDto>> GetAllForAdminAsync(int? departmentId, string? status)
    {
        var rows = await _newsRepository.GetAllAsync(departmentId, status);
        return rows.Select(MapToDto).ToList();
    }

    public async Task<NewsDto?> GetByIdForAdminAsync(int id)
    {
        var news = await _newsRepository.GetByIdAsync(id);
        return news is null ? null : MapToDto(news);
    }

    public async Task<ScopedResult<NewsDto>> CreateSchoolWideAsync(int userId, NewsRequestDto request)
    {
        if (!ContentStatus.IsValid(request.Status))
        {
            return ScopedResult<NewsDto>.Invalid($"Status must be one of: {string.Join(", ", ContentStatus.All)}.");
        }

        var news = new News
        {
            Title = request.Title,
            Body = request.Body,
            DepartmentId = null,
            Status = request.Status,
            PublishAtUtc = request.PublishAtUtc,
            CreatedBy = userId,
        };

        news.Id = await _newsRepository.CreateAsync(news);

        await _activityLog.LogAsync(userId, "NewsCreated", "News", news.Id,
            JsonSerializer.Serialize(new { news.Title, news.Status, Scope = "SchoolWide" }));

        var created = await _newsRepository.GetByIdAsync(news.Id);
        return ScopedResult<NewsDto>.Ok(MapToDto(created!));
    }

    public async Task<ScopedResult<NewsDto>> UpdateSchoolWideAsync(int id, int userId, NewsRequestDto request)
    {
        var existing = await _newsRepository.GetByIdAsync(id);
        if (existing is null)
        {
            return ScopedResult<NewsDto>.NotFound();
        }

        // Editing department News is the Academic Head's job (BW-16); Super Admin manages
        // school-wide items only through this endpoint.
        if (existing.DepartmentId is not null)
        {
            return ScopedResult<NewsDto>.Forbidden();
        }

        if (!ContentStatus.IsValid(request.Status))
        {
            return ScopedResult<NewsDto>.Invalid($"Status must be one of: {string.Join(", ", ContentStatus.All)}.");
        }

        existing.Title = request.Title;
        existing.Body = request.Body;
        existing.Status = request.Status;
        existing.PublishAtUtc = request.PublishAtUtc;
        existing.UpdatedBy = userId;

        var updated = await _newsRepository.UpdateAsync(existing);
        if (!updated)
        {
            return ScopedResult<NewsDto>.NotFound();
        }

        await _activityLog.LogAsync(userId, "NewsUpdated", "News", id,
            JsonSerializer.Serialize(new { existing.Title, existing.Status, Scope = "SchoolWide" }));

        var refreshed = await _newsRepository.GetByIdAsync(id);
        return ScopedResult<NewsDto>.Ok(MapToDto(refreshed!));
    }

    public async Task<ScopedResult<bool>> DeleteSchoolWideAsync(int id, int userId)
    {
        var existing = await _newsRepository.GetByIdAsync(id);
        if (existing is null)
        {
            return ScopedResult<bool>.NotFound();
        }

        if (existing.DepartmentId is not null)
        {
            return ScopedResult<bool>.Forbidden();
        }

        var deleted = await _newsRepository.DeleteAsync(id);
        if (!deleted)
        {
            return ScopedResult<bool>.NotFound();
        }

        await _activityLog.LogAsync(userId, "NewsDeleted", "News", id,
            JsonSerializer.Serialize(new { existing.Title, Scope = "SchoolWide" }));

        return ScopedResult<bool>.Ok(true);
    }

    private static NewsDto MapToDto(News news) => new()
    {
        Id = news.Id,
        Title = news.Title,
        Body = news.Body,
        DepartmentId = news.DepartmentId,
        DepartmentName = news.DepartmentName,
        Status = news.Status,
        PublishAtUtc = news.PublishAtUtc,
        CreatedByName = news.CreatedByName,
        CreatedAt = news.CreatedAt,
        UpdatedByName = news.UpdatedByName,
        UpdatedAt = news.UpdatedAt,
    };
}
