using System.Security.Claims;
using BCAS.Api.Common;
using BCAS.Api.Model;
using BCAS.Api.Model.DTOs;
using BCAS.Api.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BCAS.Api.Controllers;

// BW-17: Super Admin, school-wide News across all departments.
// Academic Heads have no access to this controller and get HTTP 403/401 automatically
// from [Authorize(Roles = SuperAdmin)]; department News stays the Academic Head's own
// endpoint (NewsController, BW-16).
[ApiController]
[Route("api/admin/news")]
[Authorize(Roles = RoleNames.SuperAdmin)]
public class NewsAdminController : ControllerBase
{
    private readonly INewsService _newsService;

    public NewsAdminController(INewsService newsService)
    {
        _newsService = newsService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<NewsDto>>> GetAll([FromQuery] int? departmentId, [FromQuery] string? status)
    {
        if (status is not null && !ContentStatus.IsValid(status))
        {
            return BadRequest(new ErrorResponseDto($"Status must be one of: {string.Join(", ", ContentStatus.All)}."));
        }

        var news = await _newsService.GetAllForAdminAsync(departmentId, status);
        return Ok(news);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<NewsDto>> GetById(int id)
    {
        var news = await _newsService.GetByIdForAdminAsync(id);
        if (news is null)
        {
            return NotFound(new ErrorResponseDto("News article not found."));
        }

        return Ok(news);
    }

    [HttpPost]
    public async Task<ActionResult<NewsDto>> Create([FromBody] NewsRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        if (!TryGetUserId(out var userId, out var unauthorized))
        {
            return unauthorized;
        }

        var result = await _newsService.CreateSchoolWideAsync(userId, request);
        if (result.Status == ScopedAccessStatus.Ok)
        {
            return CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value);
        }

        return ToActionResult(result);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<NewsDto>> Update(int id, [FromBody] NewsRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        if (!TryGetUserId(out var userId, out var unauthorized))
        {
            return unauthorized;
        }

        var result = await _newsService.UpdateSchoolWideAsync(id, userId, request);
        return ToActionResult(result);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        if (!TryGetUserId(out var userId, out var unauthorized))
        {
            return unauthorized;
        }

        var result = await _newsService.DeleteSchoolWideAsync(id, userId);

        if (result.Status == ScopedAccessStatus.Ok)
        {
            return NoContent();
        }
        if (result.Status == ScopedAccessStatus.NotFound)
        {
            return NotFound(new ErrorResponseDto("News article not found."));
        }
        if (result.Status == ScopedAccessStatus.Forbidden)
        {
            return Forbid();
        }
        return BadRequest(new ErrorResponseDto(result.Error ?? "Invalid request."));
    }

    private ActionResult<NewsDto> ToActionResult(ScopedResult<NewsDto> result)
    {
        if (result.Status == ScopedAccessStatus.Ok)
        {
            return Ok(result.Value);
        }
        if (result.Status == ScopedAccessStatus.NotFound)
        {
            return NotFound(new ErrorResponseDto("News article not found."));
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
