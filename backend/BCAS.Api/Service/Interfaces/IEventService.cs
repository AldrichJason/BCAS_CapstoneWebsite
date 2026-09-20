using BCAS.Api.Common;
using BCAS.Api.Model.DTOs;

namespace BCAS.Api.Service.Interfaces;

public interface IEventService
{
    Task<IReadOnlyList<EventDto>> GetDepartmentEventsAsync(int departmentId, string? status);
    Task<ScopedResult<EventDto>> GetByIdAsync(int id, int departmentId);
    Task<ScopedResult<EventDto>> CreateAsync(int departmentId, int userId, EventRequestDto request);
    Task<ScopedResult<EventDto>> UpdateAsync(int id, int departmentId, int userId, EventRequestDto request);
    Task<ScopedResult<bool>> DeleteAsync(int id, int departmentId, int userId);

    // Super Admin: school-wide Events management across all departments.
    Task<IReadOnlyList<EventDto>> GetAllForAdminAsync(int? departmentId, string? status);
    Task<EventDto?> GetByIdForAdminAsync(int id);
    Task<ScopedResult<EventDto>> CreateSchoolWideAsync(int userId, EventRequestDto request);
    Task<ScopedResult<EventDto>> UpdateSchoolWideAsync(int id, int userId, EventRequestDto request);
    Task<ScopedResult<bool>> DeleteSchoolWideAsync(int id, int userId);
}
