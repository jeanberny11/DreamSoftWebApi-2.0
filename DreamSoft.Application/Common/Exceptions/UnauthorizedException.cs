namespace DreamSoft.Application.Common.Exceptions;

/// <summary>
/// Exception thrown when user is not authorized
/// </summary>
public class UnauthorizedException : ApplicationException
{
    public UnauthorizedException() : base(ErrorMessageKeys.Unauthorized) { }

    public UnauthorizedException(string resourceKey, params object[] parameters)
        : base(resourceKey, parameters) { }
}
