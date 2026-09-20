using System.Security.Claims;
using BCAS.Api.Common;
using BCAS.Api.Model;
using BCAS.Api.Model.DTOs;
using BCAS.Api.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BCAS.Api.Controllers;

// Super Admin, school-wide Announcements across all departments. Academic Heads keep
// managing their own department's Announcements through AnnouncementsController.
[ApiController]
[Route("api/admin/announcements")]
[Authorize(Roles = RoleNames.SuperAdmin)]
public class AnnouncementsAdminController : ControllerBase
{
    private readonly IAnnouncementService _announcementService;

    public AnnouncementsAdminController(IAnnouncementService announcementService)
    {
        _announcementService = announcementService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AnnouncementDto>>> GetAll([FromQuery] int? departmentId, [FromQuery] string? status)
    {
        if (status is not null && !ContentStatus.IsValid(status))
        {
            return BadRequest(new ErrorResponseDto($"Status must be one of: {string.Join(", ", ContentStatus.All)}."));
        }

        var announcements = await _announcementService.GetAllForAdminAsync(departmentId, status);
        return Ok(announcements);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<AnnouncementDto>> GetById(int id)
    {
        var announcement = await _announcementService.GetByIdForAdminAsync(id);
        if (announcement is null)
        {
            return NotFound(new ErrorResponseDto("Announcement not found."));
        }

        return Ok(announcement);
    }

    [HttpPost]
    public async Task<ActionResult<AnnouncementDto>> Create([FromBody] AnnouncementRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        if (!TryGetUserId(out var userId, out var unauthorized))
        {
            return unauthorized;
        }

        var result = await _announcementService.CreateSchoolWideAsync(userId, request);
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

        if (!TryGetUserId(out var userId, out var unauthorized))
        {
            return unauthorized;
        }

        var result = await _announcementService.UpdateSchoolWideAsync(id, userId, request);
        return ToActionResult(result);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        if (!TryGetUserId(out var userId, out var unauthorized))
        {
            return unauthorized;
        }

        var result = await _announcementService.DeleteSchoolWideAsync(id, userId);

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

    private bool TryGetUserId(out int userId, out ActionResult unauthorized)
    {
        userId = 0;
        unauthorized = Unauthorized(new ErrorResponseDto("Invalid session token."));

        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(userIdClaim, out userId);
    }
}
