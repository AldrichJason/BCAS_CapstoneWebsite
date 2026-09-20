using System.Text.Json;
using BCAS.Api.Common;
using BCAS.Api.Model;
using BCAS.Api.Model.DTOs;
using BCAS.Api.Repository.Interfaces;
using BCAS.Api.Service.Interfaces;

namespace BCAS.Api.Service;

/// <summary>
/// Department-scoped Events CRUD for Academic Heads (BW-19). Every write is recorded
/// to ActivityLog and cross-department access is refused before it reaches the repository.
/// </summary>
public class EventService : IEventService
{
    private readonly IEventRepository _eventRepository;
    private readonly IActivityLogRepository _activityLog;

    public EventService(IEventRepository eventRepository, IActivityLogRepository activityLog)
    {
        _eventRepository = eventRepository;
        _activityLog = activityLog;
    }

    public async Task<IReadOnlyList<EventDto>> GetDepartmentEventsAsync(int departmentId, string? status)
    {
        var rows = await _eventRepository.GetByDepartmentAsync(departmentId, status);
        return rows.Select(MapToDto).ToList();
    }

    public async Task<ScopedResult<EventDto>> GetByIdAsync(int id, int departmentId)
    {
        var evt = await _eventRepository.GetByIdAsync(id);
        if (evt is null)
        {
            return ScopedResult<EventDto>.NotFound();
        }

        if (evt.DepartmentId != departmentId)
        {
            return ScopedResult<EventDto>.Forbidden();
        }

        return ScopedResult<EventDto>.Ok(MapToDto(evt));
    }

    public async Task<ScopedResult<EventDto>> CreateAsync(int departmentId, int userId, EventRequestDto request)
    {
        if (!ContentStatus.IsValid(request.Status))
        {
            return ScopedResult<EventDto>.Invalid($"Status must be one of: {string.Join(", ", ContentStatus.All)}.");
        }

        if (request.EventEndUtc.HasValue && request.EventEndUtc.Value < request.EventStartUtc)
        {
            return ScopedResult<EventDto>.Invalid("End date/time must not precede the start date/time.");
        }

        var evt = new Event
        {
            Title = request.Title,
            Description = request.Description,
            EventStartUtc = request.EventStartUtc,
            EventEndUtc = request.EventEndUtc,
            Venue = request.Venue,
            DepartmentId = departmentId,
            Status = request.Status,
            CreatedBy = userId,
        };

        evt.Id = await _eventRepository.CreateAsync(evt);

        await _activityLog.LogAsync(userId, "EventCreated", "Event", evt.Id,
            JsonSerializer.Serialize(new { evt.Title, evt.Status, DepartmentId = departmentId }));

        var created = await _eventRepository.GetByIdAsync(evt.Id);
        return ScopedResult<EventDto>.Ok(MapToDto(created!));
    }

    public async Task<ScopedResult<EventDto>> UpdateAsync(int id, int departmentId, int userId, EventRequestDto request)
    {
        var existing = await _eventRepository.GetByIdAsync(id);
        if (existing is null)
        {
            return ScopedResult<EventDto>.NotFound();
        }

        if (existing.DepartmentId != departmentId)
        {
            return ScopedResult<EventDto>.Forbidden();
        }

        if (!ContentStatus.IsValid(request.Status))
        {
            return ScopedResult<EventDto>.Invalid($"Status must be one of: {string.Join(", ", ContentStatus.All)}.");
        }

        if (request.EventEndUtc.HasValue && request.EventEndUtc.Value < request.EventStartUtc)
        {
            return ScopedResult<EventDto>.Invalid("End date/time must not precede the start date/time.");
        }

        existing.Title = request.Title;
        existing.Description = request.Description;
        existing.EventStartUtc = request.EventStartUtc;
        existing.EventEndUtc = request.EventEndUtc;
        existing.Venue = request.Venue;
        existing.Status = request.Status;
        existing.UpdatedBy = userId;

        var updated = await _eventRepository.UpdateAsync(existing);
        if (!updated)
        {
            return ScopedResult<EventDto>.NotFound();
        }

        await _activityLog.LogAsync(userId, "EventUpdated", "Event", id,
            JsonSerializer.Serialize(new { existing.Title, existing.Status }));

        var refreshed = await _eventRepository.GetByIdAsync(id);
        return ScopedResult<EventDto>.Ok(MapToDto(refreshed!));
    }

    public async Task<ScopedResult<bool>> DeleteAsync(int id, int departmentId, int userId)
    {
        var existing = await _eventRepository.GetByIdAsync(id);
        if (existing is null)
        {
            return ScopedResult<bool>.NotFound();
        }

        if (existing.DepartmentId != departmentId)
        {
            return ScopedResult<bool>.Forbidden();
        }

        var deleted = await _eventRepository.DeleteAsync(id);
        if (!deleted)
        {
            return ScopedResult<bool>.NotFound();
        }

        await _activityLog.LogAsync(userId, "EventDeleted", "Event", id,
            JsonSerializer.Serialize(new { existing.Title }));

        return ScopedResult<bool>.Ok(true);
    }

    private static EventDto MapToDto(Event evt) => new()
    {
        Id = evt.Id,
        Title = evt.Title,
        Description = evt.Description,
        EventStartUtc = evt.EventStartUtc,
        EventEndUtc = evt.EventEndUtc,
        Venue = evt.Venue,
        DepartmentId = evt.DepartmentId,
        DepartmentName = evt.DepartmentName,
        Status = evt.Status,
        CreatedByName = evt.CreatedByName,
        CreatedAt = evt.CreatedAt,
        UpdatedByName = evt.UpdatedByName,
        UpdatedAt = evt.UpdatedAt,
    };
}
