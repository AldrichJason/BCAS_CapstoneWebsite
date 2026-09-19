using System.ComponentModel.DataAnnotations;

namespace BCAS.Api.Model.DTOs;

public class CreateAccountRequestDto
{
    [Required, MaxLength(200)]
    public string FullName { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    /// <summary>One of <see cref="RoleNames"/>.</summary>
    [Required]
    public string Role { get; set; } = string.Empty;

    /// <summary>Required when Role is AcademicHead; must be null for every other role.</summary>
    public int? DepartmentId { get; set; }
}
