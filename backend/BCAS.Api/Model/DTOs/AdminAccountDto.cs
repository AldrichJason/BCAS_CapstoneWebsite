namespace BCAS.Api.Model.DTOs;

public class AdminAccountDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public int? DepartmentId { get; set; }
    public string? DepartmentName { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }

    /// <summary>Local-dev-only convenience; see <see cref="MessageResponseDto.DevPreviewCode"/>.</summary>
    public string? InviteCode { get; set; }
}
