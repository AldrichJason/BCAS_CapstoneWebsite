using BCAS.Api.Model.DTOs;

namespace BCAS.Api.Service.Interfaces;

public enum CreateAccountResult
{
    Success,
    DuplicateEmail,
    DuplicateUsername,
    InvalidRole,
    DepartmentRequired,
    DepartmentNotApplicable,
    DepartmentNotFound,
    PasswordPolicyViolation,
    PasswordMismatch,
}

public enum SetActiveResult
{
    Success,
    NotFound,
    CannotModifyOwnAccount,
}

public interface IAdminAccountService
{
    Task<IReadOnlyList<AdminAccountDto>> ListAccountsAsync();

    Task<(CreateAccountResult Result, AdminAccountDto? Account)> CreateAccountAsync(
        CreateAccountRequestDto request, int createdByUserId);

    Task<SetActiveResult> SetActiveAsync(int accountId, bool isActive, int actingUserId);
}
