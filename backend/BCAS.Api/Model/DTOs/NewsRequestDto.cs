using System.ComponentModel.DataAnnotations;
using BCAS.Api.Model;

namespace BCAS.Api.Model.DTOs;

/// <summary>
/// Create/update payload for department News (BW-16). DepartmentId is intentionally
/// absent: the department is derived server-side from the caller's own scope.
/// </summary>
public class NewsRequestDto
{
    [Required(ErrorMessage = "Title is required.")]
    [MaxLength(300)]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Body is required.")]
    public string Body { get; set; } = string.Empty;

    [Required(ErrorMessage = "Publish date is required.")]
    public DateTime PublishAtUtc { get; set; }

    [Required(ErrorMessage = "Status is required.")]
    public string Status { get; set; } = ContentStatus.Draft;
}
