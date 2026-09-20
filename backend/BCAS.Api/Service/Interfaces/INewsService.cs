using BCAS.Api.Common;
using BCAS.Api.Model.DTOs;

namespace BCAS.Api.Service.Interfaces;

public interface INewsService
{
    Task<IReadOnlyList<NewsDto>> GetDepartmentNewsAsync(int departmentId, string? status);
    Task<ScopedResult<NewsDto>> GetByIdAsync(int id, int departmentId);
    Task<ScopedResult<NewsDto>> CreateAsync(int departmentId, int userId, NewsRequestDto request);
    Task<ScopedResult<NewsDto>> UpdateAsync(int id, int departmentId, int userId, NewsRequestDto request);
    Task<ScopedResult<bool>> DeleteAsync(int id, int departmentId, int userId);

    // BW-17: Super Admin, school-wide News management across all departments.
    Task<IReadOnlyList<NewsDto>> GetAllForAdminAsync(int? departmentId, string? status);
    Task<NewsDto?> GetByIdForAdminAsync(int id);
    Task<ScopedResult<NewsDto>> CreateSchoolWideAsync(int userId, NewsRequestDto request);
    Task<ScopedResult<NewsDto>> UpdateSchoolWideAsync(int id, int userId, NewsRequestDto request);
    Task<ScopedResult<bool>> DeleteSchoolWideAsync(int id, int userId);
}
