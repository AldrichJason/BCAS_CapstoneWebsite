using BCAS.Api.Model;

namespace BCAS.Api.Repository.Interfaces;

public interface IEventRepository
{
    Task<Event?> GetByIdAsync(int id);

    /// <summary>
    /// Lists Events for a department, optionally filtered by status.
    /// </summary>
    Task<IReadOnlyList<Event>> GetByDepartmentAsync(int departmentId, string? status);

    Task<int> CreateAsync(Event evt);

    /// <summary>Returns false if the row no longer exists.</summary>
    Task<bool> UpdateAsync(Event evt);

    /// <summary>Returns false if the row no longer exists.</summary>
    Task<bool> DeleteAsync(int id);
}
