using System.Security.Claims;
using BCAS.Api.Model;
using BCAS.Api.Model.DTOs;
using BCAS.Api.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BCAS.Api.Controllers;

// BW-14 (provisioning) and BW-15 (activation toggle): Super Admin only.
[ApiController]
[Route("api/admin/accounts")]
[Authorize(Roles = RoleNames.SuperAdmin)]
public class AdminAccountsController : ControllerBase
{
    private readonly IAdminAccountService _adminAccountService;

    public AdminAccountsController(IAdminAccountService adminAccountService)
    {
        _adminAccountService = adminAccountService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AdminAccountDto>>> List()
    {
        return Ok(await _adminAccountService.ListAccountsAsync());
    }

    [HttpPost]
    public async Task<ActionResult<AdminAccountDto>> Create([FromBody] CreateAccountRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var (result, account) = await _adminAccountService.CreateAccountAsync(request, CurrentUserId());

        return result switch
        {
            CreateAccountResult.Success => CreatedAtAction(nameof(List), null, account),
            CreateAccountResult.DuplicateEmail => Conflict(new ErrorResponseDto("An account with that email already exists.")),
            CreateAccountResult.DuplicateUsername => Conflict(new ErrorResponseDto("An account with that username already exists.")),
            CreateAccountResult.InvalidRole => BadRequest(new ErrorResponseDto("Unknown role. Must be one of: SuperAdmin, AcademicHead, AdminOfficeRegistrar, VpOfOperations.")),
            CreateAccountResult.DepartmentRequired => BadRequest(new ErrorResponseDto("Department is required for the Academic Head role.")),
            CreateAccountResult.DepartmentNotApplicable => BadRequest(new ErrorResponseDto("Department does not apply to this role.")),
            CreateAccountResult.DepartmentNotFound => BadRequest(new ErrorResponseDto("Selected department does not exist.")),
            CreateAccountResult.PasswordPolicyViolation => BadRequest(new ErrorResponseDto("Password must be at least 8 characters and include a letter and a number.")),
            CreateAccountResult.PasswordMismatch => BadRequest(new ErrorResponseDto("Passwords do not match.")),
            _ => BadRequest(new ErrorResponseDto("Could not create the account.")),
        };
    }

    [HttpPatch("{id:int}/status")]
    public async Task<IActionResult> SetActive(int id, [FromBody] SetAccountActiveRequestDto request)
    {
        var result = await _adminAccountService.SetActiveAsync(id, request.IsActive, CurrentUserId());

        return result switch
        {
            SetActiveResult.Success => NoContent(),
            SetActiveResult.NotFound => NotFound(new ErrorResponseDto("Account not found.")),
            SetActiveResult.CannotModifyOwnAccount => BadRequest(new ErrorResponseDto("You cannot deactivate your own account.")),
            _ => BadRequest(new ErrorResponseDto("Could not update the account.")),
        };
    }

    private int CurrentUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
