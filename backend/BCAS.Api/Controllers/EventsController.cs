using System.Security.Claims;
using BCAS.Api.Common;
using BCAS.Api.Model;
using BCAS.Api.Model.DTOs;
using BCAS.Api.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BCAS.Api.Controllers;

// BW-19: Department-scoped School Events CRUD for Academic Heads.
[ApiController]
[Route("api/events")]
[Authorize(Roles = RoleNames.AcademicHead)]
public class EventsController : ControllerBase
{
    private readonly IEventService _eventService;

    public EventsController(IEventService eventService)
    {
        _eventService = eventService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<EventDto>>> GetAll([FromQuery] string? status)
    {
        if (!TryGetCallerContext(out var departmentId, out _, out var forbidden))
        {
            return forbidden;
        }

        if (status is not null && !ContentStatus.IsValid(status))
        {
            return BadRequest(new ErrorResponseDto($"Status must be one of: {string.Join(", ", ContentStatus.All)}."));
        }

        var events = await _eventService.GetDepartmentEventsAsync(departmentId, status);
        return Ok(events);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<EventDto>> GetById(int id)
    {
        if (!TryGetCallerContext(out var departmentId, out _, out var forbidden))
        {
            return forbidden;
        }

        var result = await _eventService.GetByIdAsync(id, departmentId);
        return ToActionResult(result);
    }

    [HttpPost]
    public async Task<ActionResult<EventDto>> Create([FromBody] EventRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        if (!TryGetCallerContext(out var departmentId, out var userId, out var forbidden))
        {
            return forbidden;
        }

        var result = await _eventService.CreateAsync(departmentId, userId, request);
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

        if (!TryGetCallerContext(out var departmentId, out var userId, out var forbidden))
        {
            return forbidden;
        }

        var result = await _eventService.UpdateAsync(id, departmentId, userId, request);
        return ToActionResult(result);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        if (!TryGetCallerContext(out var departmentId, out var userId, out var forbidden))
        {
            return forbidden;
        }

        var result = await _eventService.DeleteAsync(id, departmentId, userId);

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
