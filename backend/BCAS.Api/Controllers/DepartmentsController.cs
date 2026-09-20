using BCAS.Api.Model;
using BCAS.Api.Repository.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BCAS.Api.Controllers;

// Reference data for admin UIs (department filters/pickers). Any authenticated staff
// account may read the department list; it carries no sensitive data.
[ApiController]
[Route("api/departments")]
[Authorize]
public class DepartmentsController : ControllerBase
{
    private readonly IDepartmentRepository _departmentRepository;

    public DepartmentsController(IDepartmentRepository departmentRepository)
    {
        _departmentRepository = departmentRepository;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<Department>>> GetAll()
    {
        var departments = await _departmentRepository.GetAllAsync();
        return Ok(departments);
    }
}
