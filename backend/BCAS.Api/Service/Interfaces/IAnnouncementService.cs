using BCAS.Api.Common;
using BCAS.Api.Model.DTOs;

namespace BCAS.Api.Service.Interfaces;

public interface IAnnouncementService
{
    Task<IReadOnlyList<AnnouncementDto>> GetDepartmentAnnouncementsAsync(int departmentId, string? status, bool sortDescending);
    Task<ScopedResult<AnnouncementDto>> GetByIdAsync(int id, int departmentId);
    Task<ScopedResult<AnnouncementDto>> CreateAsync(int departmentId, int userId, AnnouncementRequestDto request);
    Task<ScopedResult<AnnouncementDto>> UpdateAsync(int id, int departmentId, int userId, AnnouncementRequestDto request);
    Task<ScopedResult<bool>> DeleteAsync(int id, int departmentId, int userId);

    // Super Admin: school-wide Announcements management across all departments.
    Task<IReadOnlyList<AnnouncementDto>> GetAllForAdminAsync(int? departmentId, string? status);
    Task<AnnouncementDto?> GetByIdForAdminAsync(int id);
    Task<ScopedResult<AnnouncementDto>> CreateSchoolWideAsync(int userId, AnnouncementRequestDto request);
    Task<ScopedResult<AnnouncementDto>> UpdateSchoolWideAsync(int id, int userId, AnnouncementRequestDto request);
    Task<ScopedResult<bool>> DeleteSchoolWideAsync(int id, int userId);
}
