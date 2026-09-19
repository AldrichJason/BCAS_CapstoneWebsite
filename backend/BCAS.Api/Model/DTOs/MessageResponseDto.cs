namespace BCAS.Api.Model.DTOs;

public class MessageResponseDto
{
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Local-dev-only convenience: the 6-digit code, present only when no SMTP
    /// server is configured and the API is running in Development. Always
    /// null in production — the real code only ever goes out by email there.
    /// </summary>
    public string? DevPreviewCode { get; set; }

    public MessageResponseDto() { }

    public MessageResponseDto(string message, string? devPreviewCode = null)
    {
        Message = message;
        DevPreviewCode = devPreviewCode;
    }
}
