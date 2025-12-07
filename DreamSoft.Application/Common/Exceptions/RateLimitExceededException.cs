namespace DreamSoft.Application.Common.Exceptions;

/// <summary>
/// Exception thrown when rate limit is exceeded
/// </summary>
public class RateLimitExceededException : ApplicationException
{
    public TimeSpan RetryAfter { get; }

    public RateLimitExceededException(string message, TimeSpan retryAfter)
        : base(message)
    {
        RetryAfter = retryAfter;
    }

    public RateLimitExceededException(string message)
        : base(message)
    {
        RetryAfter = TimeSpan.FromMinutes(5);
    }
}