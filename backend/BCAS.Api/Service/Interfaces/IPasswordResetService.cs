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
    /// Returns the 6-digit code only as a local-dev preview (Development environment,
    /// no SMTP configured); null in every other case, including production.
    /// </summary>
    Task<string?> RequestPasswordResetAsync(string email);

    /// <summary>
    /// Issues a set-your-password code for a newly provisioned account. Returns the
    /// code only as a local-dev preview, under the same conditions as above.
    /// </summary>
    Task<string?> IssueInviteAsync(User newUser);

    Task<ResetPasswordResult> ResetPasswordAsync(string email, string code, string newPassword);
}
