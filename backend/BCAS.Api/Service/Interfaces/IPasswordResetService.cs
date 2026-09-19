using BCAS.Api.Model;

namespace BCAS.Api.Service.Interfaces;

public enum ResetPasswordResult
{
    Success,
    InvalidOrExpiredToken,
}

public interface IPasswordResetService
{
    /// <summary>
    /// Always succeeds from the caller's point of view — silently no-ops for an
    /// unknown or inactive email so the response can't be used to enumerate accounts.
    /// </summary>
    Task RequestPasswordResetAsync(string email);

    /// <summary>Issues a set-your-password link for a newly provisioned account.</summary>
    Task IssueInviteAsync(User newUser);

    Task<ResetPasswordResult> ResetPasswordAsync(string token, string newPassword);
}
