namespace BCAS.Api.Model;

/// <summary>
/// Shared Draft/Scheduled/Published/Archived workflow used by News, Announcements,
/// Events and the other content tables (BW-9 schema).
/// </summary>
public static class ContentStatus
{
    public const string Draft = "Draft";
    public const string Scheduled = "Scheduled";
    public const string Published = "Published";
    public const string Archived = "Archived";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        Draft, Scheduled, Published, Archived,
    };

    public static bool IsValid(string? status) => status is not null && All.Contains(status);
}
