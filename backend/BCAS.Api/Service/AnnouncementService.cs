using System.Text.Json;
using BCAS.Api.Common;
using BCAS.Api.Model;
using BCAS.Api.Model.DTOs;
using BCAS.Api.Repository.Interfaces;
using BCAS.Api.Service.Interfaces;

namespace BCAS.Api.Service;

/// <summary>
/// Department-scoped Announcements CRUD for Academic Heads (BW-18). Every write is
/// recorded to ActivityLog and cross-department access is refused before it reaches
/// the repository.
/// </summary>
public class AnnouncementService : IAnnouncementService
{
    private readonly IAnnouncementRepository _announcementRepository;
    private readonly IActivityLogRepository _activityLog;

    public AnnouncementService(IAnnouncementRepository announcementRepository, IActivityLogRepository activityLog)
    {
        _announcementRepository = announcementRepository;
        _activityLog = activityLog;
    }

    public async Task<IReadOnlyList<AnnouncementDto>> GetDepartmentAnnouncementsAsync(int departmentId, string? status, bool sortDescending)
    {
        var rows = await _announcementRepository.GetByDepartmentAsync(departmentId, status, sortDescending);
        return rows.Select(MapToDto).ToList();
    }

    public async Task<ScopedResult<AnnouncementDto>> GetByIdAsync(int id, int departmentId)
    {
        var announcement = await _announcementRepository.GetByIdAsync(id);
        if (announcement is null)
        {
            return ScopedResult<AnnouncementDto>.NotFound();
        }

        if (announcement.DepartmentId != departmentId)
        {
            return ScopedResult<AnnouncementDto>.Forbidden();
        }

        return ScopedResult<AnnouncementDto>.Ok(MapToDto(announcement));
    }

    public async Task<ScopedResult<AnnouncementDto>> CreateAsync(int departmentId, int userId, AnnouncementRequestDto request)
    {
        if (!ContentStatus.IsValid(request.Status))
        {
            return ScopedResult<AnnouncementDto>.Invalid($"Status must be one of: {string.Join(", ", ContentStatus.All)}.");
        }

        var announcement = new Announcement
        {
            Title = request.Title,
            Body = request.Body,
            DepartmentId = departmentId,
            Status = request.Status,
            PublishAtUtc = request.EffectiveDateUtc,
            CreatedBy = userId,
        };

        announcement.Id = await _announcementRepository.CreateAsync(announcement);

        await _activityLog.LogAsync(userId, "AnnouncementCreated", "Announcement", announcement.Id,
            JsonSerializer.Serialize(new { announcement.Title, announcement.Status, DepartmentId = departmentId }));

        var created = await _announcementRepository.GetByIdAsync(announcement.Id);
        return ScopedResult<AnnouncementDto>.Ok(MapToDto(created!));
    }

    public async Task<ScopedResult<AnnouncementDto>> UpdateAsync(int id, int departmentId, int userId, AnnouncementRequestDto request)
    {
        var existing = await _announcementRepository.GetByIdAsync(id);
        if (existing is null)
        {
            return ScopedResult<AnnouncementDto>.NotFound();
        }

        if (existing.DepartmentId != departmentId)
        {
            return ScopedResult<AnnouncementDto>.Forbidden();
        }

        if (!ContentStatus.IsValid(request.Status))
        {
            return ScopedResult<AnnouncementDto>.Invalid($"Status must be one of: {string.Join(", ", ContentStatus.All)}.");
        }

        existing.Title = request.Title;
        existing.Body = request.Body;
        existing.Status = request.Status;
        existing.PublishAtUtc = request.EffectiveDateUtc;
        existing.UpdatedBy = userId;

        var updated = await _announcementRepository.UpdateAsync(existing);
        if (!updated)
        {
            return ScopedResult<AnnouncementDto>.NotFound();
        }

        await _activityLog.LogAsync(userId, "AnnouncementUpdated", "Announcement", id,
            JsonSerializer.Serialize(new { existing.Title, existing.Status }));

        var refreshed = await _announcementRepository.GetByIdAsync(id);
        return ScopedResult<AnnouncementDto>.Ok(MapToDto(refreshed!));
    }

    public async Task<ScopedResult<bool>> DeleteAsync(int id, int departmentId, int userId)
    {
        var existing = await _announcementRepository.GetByIdAsync(id);
        if (existing is null)
        {
            return ScopedResult<bool>.NotFound();
        }

        if (existing.DepartmentId != departmentId)
        {
            return ScopedResult<bool>.Forbidden();
        }

        var deleted = await _announcementRepository.DeleteAsync(id);
        if (!deleted)
        {
            return ScopedResult<bool>.NotFound();
        }

        await _activityLog.LogAsync(userId, "AnnouncementDeleted", "Announcement", id,
            JsonSerializer.Serialize(new { existing.Title }));

        return ScopedResult<bool>.Ok(true);
    }

    public async Task<IReadOnlyList<AnnouncementDto>> GetAllForAdminAsync(int? departmentId, string? status)
    {
        var rows = await _announcementRepository.GetAllAsync(departmentId, status);
        return rows.Select(MapToDto).ToList();
    }

    public async Task<AnnouncementDto?> GetByIdForAdminAsync(int id)
    {
        var announcement = await _announcementRepository.GetByIdAsync(id);
        return announcement is null ? null : MapToDto(announcement);
    }

    public async Task<ScopedResult<AnnouncementDto>> CreateSchoolWideAsync(int userId, AnnouncementRequestDto request)
    {
        if (!ContentStatus.IsValid(request.Status))
        {
            return ScopedResult<AnnouncementDto>.Invalid($"Status must be one of: {string.Join(", ", ContentStatus.All)}.");
        }

        var announcement = new Announcement
        {
            Title = request.Title,
            Body = request.Body,
            DepartmentId = null,
            Status = request.Status,
            PublishAtUtc = request.EffectiveDateUtc,
            CreatedBy = userId,
        };

        announcement.Id = await _announcementRepository.CreateAsync(announcement);

        await _activityLog.LogAsync(userId, "AnnouncementCreated", "Announcement", announcement.Id,
            JsonSerializer.Serialize(new { announcement.Title, announcement.Status, Scope = "SchoolWide" }));

        var created = await _announcementRepository.GetByIdAsync(announcement.Id);
        return ScopedResult<AnnouncementDto>.Ok(MapToDto(created!));
    }

    public async Task<ScopedResult<AnnouncementDto>> UpdateSchoolWideAsync(int id, int userId, AnnouncementRequestDto request)
    {
        var existing = await _announcementRepository.GetByIdAsync(id);
        if (existing is null)
        {
            return ScopedResult<AnnouncementDto>.NotFound();
        }

        if (existing.DepartmentId is not null)
        {
            return ScopedResult<AnnouncementDto>.Forbidden();
        }

        if (!ContentStatus.IsValid(request.Status))
        {
            return ScopedResult<AnnouncementDto>.Invalid($"Status must be one of: {string.Join(", ", ContentStatus.All)}.");
        }

        existing.Title = request.Title;
        existing.Body = request.Body;
        existing.Status = request.Status;
        existing.PublishAtUtc = request.EffectiveDateUtc;
        existing.UpdatedBy = userId;

        var updated = await _announcementRepository.UpdateAsync(existing);
        if (!updated)
        {
            return ScopedResult<AnnouncementDto>.NotFound();
        }

        await _activityLog.LogAsync(userId, "AnnouncementUpdated", "Announcement", id,
            JsonSerializer.Serialize(new { existing.Title, existing.Status, Scope = "SchoolWide" }));

        var refreshed = await _announcementRepository.GetByIdAsync(id);
        return ScopedResult<AnnouncementDto>.Ok(MapToDto(refreshed!));
    }

    public async Task<ScopedResult<bool>> DeleteSchoolWideAsync(int id, int userId)
    {
        var existing = await _announcementRepository.GetByIdAsync(id);
        if (existing is null)
        {
            return ScopedResult<bool>.NotFound();
        }

        if (existing.DepartmentId is not null)
        {
            return ScopedResult<bool>.Forbidden();
        }

        var deleted = await _announcementRepository.DeleteAsync(id);
        if (!deleted)
        {
            return ScopedResult<bool>.NotFound();
        }

        await _activityLog.LogAsync(userId, "AnnouncementDeleted", "Announcement", id,
            JsonSerializer.Serialize(new { existing.Title, Scope = "SchoolWide" }));

        return ScopedResult<bool>.Ok(true);
    }

    private static AnnouncementDto MapToDto(Announcement announcement) => new()
    {
        Id = announcement.Id,
        Title = announcement.Title,
        Body = announcement.Body,
        DepartmentId = announcement.DepartmentId,
        DepartmentName = announcement.DepartmentName,
        Status = announcement.Status,
        EffectiveDateUtc = announcement.PublishAtUtc,
        CreatedByName = announcement.CreatedByName,
        CreatedAt = announcement.CreatedAt,
        UpdatedByName = announcement.UpdatedByName,
        UpdatedAt = announcement.UpdatedAt,
    };
}
