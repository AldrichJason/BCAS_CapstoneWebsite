using System.ComponentModel.DataAnnotations;

namespace BCAS.Api.Model.DTOs;

public class CreateAccountRequestDto
{
    [Required, MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string Username { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    /// <summary>One of <see cref="RoleNames"/>.</summary>
    [Required]
    public string Role { get; set; } = string.Empty;

    /// <summary>Required when Role is AcademicHead; must be null for every other role.</summary>
    public int? DepartmentId { get; set; }

    /// <summary>
    /// Optional. When set, the Super Admin is choosing the initial password directly
    /// instead of emailing an invite code — must satisfy the same password policy as
    /// a self-service reset and match <see cref="ConfirmPassword"/>. Leave both blank
    /// (null or empty) to fall back to the invite-code flow. Validated in
    /// AdminAccountService, not here, so an empty string is treated the same as
    /// omitted rather than failing a "must be 8+ characters" check.
    /// </summary>
    public string? Password { get; set; }

    public string? ConfirmPassword { get; set; }
}
