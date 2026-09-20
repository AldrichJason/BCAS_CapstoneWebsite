using BCAS.Api.Model;

namespace BCAS.Api.Repository.Interfaces;

public interface IAnnouncementRepository
{
    Task<Announcement?> GetByIdAsync(int id);

    /// <summary>
    /// Lists Announcements for a department, optionally filtered by status and sorted by
    /// PublishAtUtc (BW-18 AC: "filterable by status and sortable by date").
    /// </summary>
    Task<IReadOnlyList<Announcement>> GetByDepartmentAsync(int departmentId, string? status, bool sortDescending);

    /// <summary>
    /// Super Admin cross-department view. <paramref name="departmentId"/>: null = every
    /// department plus school-wide; 0 = school-wide only; a real department id = that department only.
    /// </summary>
    Task<IReadOnlyList<Announcement>> GetAllAsync(int? departmentId, string? status);

    Task<int> CreateAsync(Announcement announcement);

    /// <summary>Returns false if the row no longer exists.</summary>
    Task<bool> UpdateAsync(Announcement announcement);

    /// <summary>Returns false if the row no longer exists.</summary>
    Task<bool> DeleteAsync(int id);
}
