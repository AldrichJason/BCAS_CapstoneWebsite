using System.ComponentModel.DataAnnotations;
using BCAS.Api.Model;

namespace BCAS.Api.Model.DTOs;

/// <summary>
/// Create/update payload for department Events (BW-19). DepartmentId is intentionally
/// absent: the department is derived server-side from the caller's own scope.
/// </summary>
public class EventRequestDto : IValidatableObject
{
    [Required(ErrorMessage = "Title is required.")]
    [MaxLength(300)]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Description is required.")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Start date/time is required.")]
    public DateTime EventStartUtc { get; set; }

    public DateTime? EventEndUtc { get; set; }

    [MaxLength(300)]
    public string? Venue { get; set; }

    [Required(ErrorMessage = "Status is required.")]
    public string Status { get; set; } = ContentStatus.Draft;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (EventEndUtc.HasValue && EventEndUtc.Value < EventStartUtc)
        {
            yield return new ValidationResult(
                "End date/time must not precede the start date/time.",
                new[] { nameof(EventEndUtc) });
        }
    }
}
