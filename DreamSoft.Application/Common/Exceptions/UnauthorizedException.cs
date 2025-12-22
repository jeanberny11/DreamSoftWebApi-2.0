namespace DreamSoft.Application.Common.Exceptions;

/// <summary>
/// Exception thrown when user is not authorized
/// </summary>
public class UnauthorizedException(string resourceKey, params object[] parameters) : ApplicationException(resourceKey, parameters)
{
}
