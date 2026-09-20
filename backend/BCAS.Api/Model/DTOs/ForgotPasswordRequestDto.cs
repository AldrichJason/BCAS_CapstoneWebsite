using System.ComponentModel.DataAnnotations;

namespace BCAS.Api.Model.DTOs;

public class ForgotPasswordRequestDto
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;
}
