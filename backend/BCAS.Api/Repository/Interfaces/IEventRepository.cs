using BCAS.Api.Model;

namespace BCAS.Api.Repository.Interfaces;

public interface IEventRepository
{
    Task<Event?> GetByIdAsync(int id);

    /// <summary>
    /// Lists Events for a department, optionally filtered by status.
    /// </summary>
    Task<IReadOnlyList<Event>> GetByDepartmentAsync(int departmentId, string? status);

    /// <summary>
    /// Super Admin cross-department view. <paramref name="departmentId"/>: null = every
    /// department plus school-wide; 0 = school-wide only; a real department id = that department only.
    /// </summary>
    Task<IReadOnlyList<Event>> GetAllAsync(int? departmentId, string? status);

    Task<int> CreateAsync(Event evt);

    /// <summary>Returns false if the row no longer exists.</summary>
    Task<bool> UpdateAsync(Event evt);

    /// <summary>Returns false if the row no longer exists.</summary>
    Task<bool> DeleteAsync(int id);
}
