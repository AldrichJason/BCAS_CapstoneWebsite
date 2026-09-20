using System.Security.Claims;
using BCAS.Api.Common;
using BCAS.Api.Model;
using BCAS.Api.Model.DTOs;
using BCAS.Api.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BCAS.Api.Controllers;

// BW-16: Department-scoped News CRUD for Academic Heads.
[ApiController]
[Route("api/news")]
[Authorize(Roles = RoleNames.AcademicHead)]
public class NewsController : ControllerBase
{
    private readonly INewsService _newsService;

    public NewsController(INewsService newsService)
    {
        _newsService = newsService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<NewsDto>>> GetAll([FromQuery] string? status)
    {
        if (!TryGetCallerContext(out var departmentId, out _, out var forbidden))
        {
            return forbidden;
        }

        if (status is not null && !ContentStatus.IsValid(status))
        {
            return BadRequest(new ErrorResponseDto($"Status must be one of: {string.Join(", ", ContentStatus.All)}."));
        }

        var news = await _newsService.GetDepartmentNewsAsync(departmentId, status);
        return Ok(news);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<NewsDto>> GetById(int id)
    {
        if (!TryGetCallerContext(out var departmentId, out _, out var forbidden))
        {
            return forbidden;
        }

        var result = await _newsService.GetByIdAsync(id, departmentId);
        return ToActionResult(result);
    }

    [HttpPost]
    public async Task<ActionResult<NewsDto>> Create([FromBody] NewsRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        if (!TryGetCallerContext(out var departmentId, out var userId, out var forbidden))
        {
            return forbidden;
        }

        var result = await _newsService.CreateAsync(departmentId, userId, request);
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

        if (!TryGetCallerContext(out var departmentId, out var userId, out var forbidden))
        {
            return forbidden;
        }

        var result = await _newsService.UpdateAsync(id, departmentId, userId, request);
        return ToActionResult(result);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        if (!TryGetCallerContext(out var departmentId, out var userId, out var forbidden))
        {
            return forbidden;
        }

        var result = await _newsService.DeleteAsync(id, departmentId, userId);

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

    /// <summary>
    /// Resolves the caller's own department/user id from JWT claims. Academic Heads always
    /// carry a departmentId claim (enforced at account creation); a token missing it is
    /// treated as forbidden rather than allowed to fall through to school-wide content.
    /// </summary>
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
