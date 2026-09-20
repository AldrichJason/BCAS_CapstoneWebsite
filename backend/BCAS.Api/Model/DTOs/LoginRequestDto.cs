using System.ComponentModel.DataAnnotations;

namespace BCAS.Api.Model.DTOs;

public class LoginRequestDto
{
    /// <summary>Either the account's email address or its username.</summary>
    [Required]
    public string EmailOrUsername { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}
