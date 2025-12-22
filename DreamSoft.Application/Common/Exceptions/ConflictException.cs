namespace DreamSoft.Application.Common.Exceptions;

/// <summary>
/// Exception thrown when a resource already exists (duplicate)
/// </summary>
public class ConflictException(string resourceKey, params object[] parameters) : ApplicationException(resourceKey, parameters)
{
}
