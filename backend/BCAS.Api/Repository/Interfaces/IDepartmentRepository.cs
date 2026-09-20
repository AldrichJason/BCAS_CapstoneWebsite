using BCAS.Api.Model;

namespace BCAS.Api.Repository.Interfaces;

public interface IDepartmentRepository
{
    Task<IReadOnlyList<Department>> GetAllAsync();
}
