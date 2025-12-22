namespace DreamSoft.Application.Common.Exceptions;

/// <summary>
/// Exception thrown when rate limit is exceeded
/// </summary>
public class RateLimitExceededException(string resourceKey, params object[] parameters) : ApplicationException(resourceKey, parameters)
{
}
