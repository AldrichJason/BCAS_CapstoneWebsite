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
}
