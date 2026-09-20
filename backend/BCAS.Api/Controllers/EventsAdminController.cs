using System.Security.Claims;
using BCAS.Api.Common;
using BCAS.Api.Model;
using BCAS.Api.Model.DTOs;
using BCAS.Api.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BCAS.Api.Controllers;

// Super Admin, school-wide Events across all departments. Academic Heads keep managing
// their own department's Events through EventsController.
[ApiController]
[Route("api/admin/events")]
[Authorize(Roles = RoleNames.SuperAdmin)]
public class EventsAdminController : ControllerBase
{
    private readonly IEventService _eventService;

    public EventsAdminController(IEventService eventService)
    {
        _eventService = eventService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<EventDto>>> GetAll([FromQuery] int? departmentId, [FromQuery] string? status)
    {
        if (status is not null && !ContentStatus.IsValid(status))
        {
            return BadRequest(new ErrorResponseDto($"Status must be one of: {string.Join(", ", ContentStatus.All)}."));
        }

        var events = await _eventService.GetAllForAdminAsync(departmentId, status);
        return Ok(events);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<EventDto>> GetById(int id)
    {
        var evt = await _eventService.GetByIdForAdminAsync(id);
        if (evt is null)
        {
            return NotFound(new ErrorResponseDto("Event not found."));
        }

        return Ok(evt);
    }

    [HttpPost]
    public async Task<ActionResult<EventDto>> Create([FromBody] EventRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        if (!TryGetUserId(out var userId, out var unauthorized))
        {
            return unauthorized;
        }

        var result = await _eventService.CreateSchoolWideAsync(userId, request);
        if (result.Status == ScopedAccessStatus.Ok)
        {
            return CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value);
        }

        return ToActionResult(result);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<EventDto>> Update(int id, [FromBody] EventRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        if (!TryGetUserId(out var userId, out var unauthorized))
        {
            return unauthorized;
        }

        var result = await _eventService.UpdateSchoolWideAsync(id, userId, request);
        return ToActionResult(result);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        if (!TryGetUserId(out var userId, out var unauthorized))
        {
            return unauthorized;
        }

        var result = await _eventService.DeleteSchoolWideAsync(id, userId);

        if (result.Status == ScopedAccessStatus.Ok)
        {
            return NoContent();
        }
        if (result.Status == ScopedAccessStatus.NotFound)
        {
            return NotFound(new ErrorResponseDto("Event not found."));
        }
        if (result.Status == ScopedAccessStatus.Forbidden)
        {
            return Forbid();
        }
        return BadRequest(new ErrorResponseDto(result.Error ?? "Invalid request."));
    }

    private ActionResult<EventDto> ToActionResult(ScopedResult<EventDto> result)
    {
        if (result.Status == ScopedAccessStatus.Ok)
        {
            return Ok(result.Value);
        }
        if (result.Status == ScopedAccessStatus.NotFound)
        {
            return NotFound(new ErrorResponseDto("Event not found."));
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
