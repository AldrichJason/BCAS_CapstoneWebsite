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
}
