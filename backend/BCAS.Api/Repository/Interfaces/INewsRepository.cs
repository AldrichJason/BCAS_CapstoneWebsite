using BCAS.Api.Model;

namespace BCAS.Api.Repository.Interfaces;

public interface INewsRepository
{
    Task<News?> GetByIdAsync(int id);

    /// <summary>
    /// Lists News for a department (school-wide items excluded), optionally filtered by status.
    /// </summary>
    Task<IReadOnlyList<News>> GetByDepartmentAsync(int departmentId, string? status);

    /// <summary>
    /// Super Admin cross-department view (BW-17). <paramref name="departmentId"/>: null = every
    /// department plus school-wide; 0 = school-wide only; a real department id = that department only.
    /// </summary>
    Task<IReadOnlyList<News>> GetAllAsync(int? departmentId, string? status);

    Task<int> CreateAsync(News news);

    /// <summary>Returns false if the row no longer exists.</summary>
    Task<bool> UpdateAsync(News news);

    /// <summary>Returns false if the row no longer exists.</summary>
    Task<bool> DeleteAsync(int id);
}
