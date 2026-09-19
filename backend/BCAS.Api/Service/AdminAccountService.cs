using System.Text.RegularExpressions;
using BCAS.Api.Helper;
using BCAS.Api.Model;
using BCAS.Api.Model.DTOs;
using BCAS.Api.Repository.Interfaces;
using BCAS.Api.Service.Interfaces;

namespace BCAS.Api.Service;

public class AdminAccountService : IAdminAccountService
{
    private static readonly Regex PasswordPolicy = new(@"^(?=.*[A-Za-z])(?=.*\d).{8,}$", RegexOptions.Compiled);

    private readonly IUserRepository _userRepository;
    private readonly IPasswordResetService _passwordResetService;
    private readonly IActivityLogRepository _activityLog;

    public AdminAccountService(
        IUserRepository userRepository,
        IPasswordResetService passwordResetService,
        IActivityLogRepository activityLog)
    {
        _userRepository = userRepository;
        _passwordResetService = passwordResetService;
        _activityLog = activityLog;
    }

    public async Task<IReadOnlyList<AdminAccountDto>> ListAccountsAsync()
    {
        var users = await _userRepository.ListAsync();
        return users.Select(MapToDto).ToList();
    }

    public async Task<(CreateAccountResult Result, AdminAccountDto? Account)> CreateAccountAsync(
        CreateAccountRequestDto request, int createdByUserId)
    {
        var role = await _userRepository.GetRoleByNameAsync(request.Role);
        if (role is null)
        {
            return (CreateAccountResult.InvalidRole, null);
        }

        var isAcademicHead = role.Name == RoleNames.AcademicHead;
        if (isAcademicHead && request.DepartmentId is null)
        {
            return (CreateAccountResult.DepartmentRequired, null);
        }
        if (!isAcademicHead && request.DepartmentId is not null)
        {
            return (CreateAccountResult.DepartmentNotApplicable, null);
        }
        if (request.DepartmentId is int departmentId && !await _userRepository.DepartmentExistsAsync(departmentId))
        {
            return (CreateAccountResult.DepartmentNotFound, null);
        }

        if (await _userRepository.EmailExistsAsync(request.Email))
        {
            return (CreateAccountResult.DuplicateEmail, null);
        }

        if (await _userRepository.UsernameExistsAsync(request.Username))
        {
            return (CreateAccountResult.DuplicateUsername, null);
        }

        // The Super Admin may set the password directly; otherwise the account
        // starts with a random, never-disclosed password and the invite code
        // is the only way to set a real one (BW-14 AC).
        var directPasswordProvided = !string.IsNullOrEmpty(request.Password);
        if (directPasswordProvided)
        {
            if (!PasswordPolicy.IsMatch(request.Password!))
            {
                return (CreateAccountResult.PasswordPolicyViolation, null);
            }
            if (request.Password != request.ConfirmPassword)
            {
                return (CreateAccountResult.PasswordMismatch, null);
            }
        }

        var (hash, salt) = directPasswordProvided
            ? PasswordHasher.HashPassword(request.Password!)
            : PasswordHasher.HashPassword(SecureTokenGenerator.GenerateToken());

        var newUserId = await _userRepository.CreateUserAsync(
            new NewUser(request.FirstName, request.LastName, request.Username, request.Email, hash, salt, role.Id, request.DepartmentId));

        var createdUser = await _userRepository.GetByIdAsync(newUserId)
            ?? throw new InvalidOperationException("User was created but could not be re-read.");

        var dto = MapToDto(createdUser);

        if (!directPasswordProvided)
        {
            dto.InviteCode = await _passwordResetService.IssueInviteAsync(createdUser);
        }

        await _activityLog.LogAsync(createdByUserId, "AccountCreated", "User", newUserId, $"Created {request.Email} ({role.Name})");

        return (CreateAccountResult.Success, dto);
    }

    public async Task<SetActiveResult> SetActiveAsync(int accountId, bool isActive, int actingUserId)
    {
        if (accountId == actingUserId && !isActive)
        {
            return SetActiveResult.CannotModifyOwnAccount;
        }

        var account = await _userRepository.GetByIdAsync(accountId);
        if (account is null)
        {
            return SetActiveResult.NotFound;
        }

        await _userRepository.SetActiveAsync(accountId, isActive);
        await _activityLog.LogAsync(actingUserId, isActive ? "AccountReactivated" : "AccountDeactivated", "User", accountId);

        return SetActiveResult.Success;
    }

    private static AdminAccountDto MapToDto(User user) => new()
    {
        Id = user.Id,
        FirstName = user.FirstName,
        LastName = user.LastName,
        Username = user.Username,
        Email = user.Email,
        Role = user.RoleName,
        DepartmentId = user.DepartmentId,
        DepartmentName = user.DepartmentName,
        IsActive = user.IsActive,
        CreatedAt = user.CreatedAt,
    };
}
