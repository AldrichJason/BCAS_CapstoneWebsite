using System.Security.Claims;
using BCAS.Api.Common;
using BCAS.Api.Model;
using BCAS.Api.Model.DTOs;
using BCAS.Api.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BCAS.Api.Controllers;

// BW-18: Department-scoped Announcements CRUD for Academic Heads.
[ApiController]
[Route("api/announcements")]
[Authorize(Roles = RoleNames.AcademicHead)]
public class AnnouncementsController : ControllerBase
{
    private readonly IAnnouncementService _announcementService;

    public AnnouncementsController(IAnnouncementService announcementService)
    {
        _announcementService = announcementService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AnnouncementDto>>> GetAll(
        [FromQuery] string? status,
        [FromQuery] string sort = "desc")
    {
        if (!TryGetCallerContext(out var departmentId, out _, out var forbidden))
        {
            return forbidden;
        }

        if (status is not null && !ContentStatus.IsValid(status))
        {
            return BadRequest(new ErrorResponseDto($"Status must be one of: {string.Join(", ", ContentStatus.All)}."));
        }

        if (!string.Equals(sort, "asc", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(sort, "desc", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(new ErrorResponseDto("Sort must be 'asc' or 'desc'."));
        }

        var sortDescending = !string.Equals(sort, "asc", StringComparison.OrdinalIgnoreCase);
        var announcements = await _announcementService.GetDepartmentAnnouncementsAsync(departmentId, status, sortDescending);
        return Ok(announcements);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<AnnouncementDto>> GetById(int id)
    {
        if (!TryGetCallerContext(out var departmentId, out _, out var forbidden))
        {
            return forbidden;
        }

        var result = await _announcementService.GetByIdAsync(id, departmentId);
        return ToActionResult(result);
    }

    [HttpPost]
    public async Task<ActionResult<AnnouncementDto>> Create([FromBody] AnnouncementRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        if (!TryGetCallerContext(out var departmentId, out var userId, out var forbidden))
        {
            return forbidden;
        }

        var result = await _announcementService.CreateAsync(departmentId, userId, request);
        if (result.Status == ScopedAccessStatus.Ok)
        {
            return CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value);
        }

        return ToActionResult(result);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<AnnouncementDto>> Update(int id, [FromBody] AnnouncementRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        if (!TryGetCallerContext(out var departmentId, out var userId, out var forbidden))
        {
            return forbidden;
        }

        var result = await _announcementService.UpdateAsync(id, departmentId, userId, request);
        return ToActionResult(result);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        if (!TryGetCallerContext(out var departmentId, out var userId, out var forbidden))
        {
            return forbidden;
        }

        var result = await _announcementService.DeleteAsync(id, departmentId, userId);

        if (result.Status == ScopedAccessStatus.Ok)
        {
            return NoContent();
        }
        if (result.Status == ScopedAccessStatus.NotFound)
        {
            return NotFound(new ErrorResponseDto("Announcement not found."));
        }
        if (result.Status == ScopedAccessStatus.Forbidden)
        {
            return Forbid();
        }
        return BadRequest(new ErrorResponseDto(result.Error ?? "Invalid request."));
    }

    private ActionResult<AnnouncementDto> ToActionResult(ScopedResult<AnnouncementDto> result)
    {
        if (result.Status == ScopedAccessStatus.Ok)
        {
            return Ok(result.Value);
        }
        if (result.Status == ScopedAccessStatus.NotFound)
        {
            return NotFound(new ErrorResponseDto("Announcement not found."));
        }
        if (result.Status == ScopedAccessStatus.Forbidden)
        {
            return Forbid();
        }
        return BadRequest(new ErrorResponseDto(result.Error ?? "Invalid request."));
    }

    private bool TryGetCallerContext(out int departmentId, out int userId, out ActionResult forbidden)
    {
        departmentId = 0;
        userId = 0;
        forbidden = Forbid();

        var departmentClaim = User.FindFirstValue("departmentId");
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(departmentClaim, out departmentId) || !int.TryParse(userIdClaim, out userId))
        {
            return false;
        }

        return true;
    }
}
