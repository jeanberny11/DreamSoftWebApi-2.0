namespace DreamSoft.Application.Common;

/// <summary>
/// Represents the outcome of an email send operation.
/// </summary>
public sealed class EmailSendResult
{
    public bool IsSuccess { get; }
    public string? Error { get; }

    private EmailSendResult(bool isSuccess, string? error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    public static EmailSendResult Success() => new(true, null);
    public static EmailSendResult Failure(string error) => new(false, error);
}
