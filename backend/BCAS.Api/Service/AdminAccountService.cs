using BCAS.Api.Helper;
using BCAS.Api.Model;
using BCAS.Api.Model.DTOs;
using BCAS.Api.Repository.Interfaces;
using BCAS.Api.Service.Interfaces;

namespace BCAS.Api.Service;

public class AdminAccountService : IAdminAccountService
{
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

        // New accounts start with a random, never-disclosed password; the
        // invite email is the only way to set a real one (BW-14 AC).
        var (hash, salt) = PasswordHasher.HashPassword(SecureTokenGenerator.GenerateToken());

        var newUserId = await _userRepository.CreateUserAsync(
            new NewUser(request.FullName, request.Email, hash, salt, role.Id, request.DepartmentId));

        var createdUser = await _userRepository.GetByIdAsync(newUserId)
            ?? throw new InvalidOperationException("User was created but could not be re-read.");

        await _passwordResetService.IssueInviteAsync(createdUser);
        await _activityLog.LogAsync(createdByUserId, "AccountCreated", "User", newUserId, $"Created {request.Email} ({role.Name})");

        return (CreateAccountResult.Success, MapToDto(createdUser));
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
        FullName = user.FullName,
        Email = user.Email,
        Role = user.RoleName,
        DepartmentId = user.DepartmentId,
        DepartmentName = user.DepartmentName,
        IsActive = user.IsActive,
        CreatedAt = user.CreatedAt,
    };
}
