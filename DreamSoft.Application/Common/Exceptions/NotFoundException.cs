namespace DreamSoft.Application.Common.Exceptions;

/// <summary>
/// Exception thrown when a requested entity is not found
/// </summary>
public class NotFoundException(string resourceKey, params object[] parameters) : ApplicationException(resourceKey, parameters)
{
}
