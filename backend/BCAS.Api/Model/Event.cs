namespace BCAS.Api.Model;

public class Event
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime EventStartUtc { get; set; }
    public DateTime? EventEndUtc { get; set; }
    public string? Venue { get; set; }
    public int? DepartmentId { get; set; }
    public string? DepartmentName { get; set; }
    public string Status { get; set; } = string.Empty;
    public int CreatedBy { get; set; }
    public string? CreatedByName { get; set; }
    public DateTime CreatedAt { get; set; }
    public int? UpdatedBy { get; set; }
    public string? UpdatedByName { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
