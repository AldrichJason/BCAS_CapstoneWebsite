namespace BCAS.Api.Common;

public enum ScopedAccessStatus
{
    Ok,
    NotFound,
    Forbidden,
    Invalid,
}

/// <summary>
/// Outcome of a department-scoped read/write so controllers can map cross-department
/// access to HTTP 403 instead of a generic 404 (BW-16/17/18/19 AC: server-side 403).
/// </summary>
public class ScopedResult<T>
{
    public ScopedAccessStatus Status { get; }
    public T? Value { get; }
    public string? Error { get; }

    private ScopedResult(ScopedAccessStatus status, T? value, string? error)
    {
        Status = status;
        Value = value;
        Error = error;
    }

    public static ScopedResult<T> Ok(T value) => new(ScopedAccessStatus.Ok, value, null);
    public static ScopedResult<T> NotFound() => new(ScopedAccessStatus.NotFound, default, null);
    public static ScopedResult<T> Forbidden() => new(ScopedAccessStatus.Forbidden, default, null);
    public static ScopedResult<T> Invalid(string error) => new(ScopedAccessStatus.Invalid, default, error);
}
