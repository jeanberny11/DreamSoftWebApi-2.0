namespace DreamSoft.Application.Common.Exceptions;

/// <summary>
/// Exception thrown when rate limit is exceeded
/// </summary>
public class RateLimitExceededException(params object[] parameters)
    : ApplicationException(ErrorMessageKeys.RateLimitExceeded, parameters) { }
