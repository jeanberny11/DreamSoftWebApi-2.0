namespace DreamSoft.Application.Common.Exceptions;

/// <summary>
/// Exception thrown when a resource already exists (duplicate)
/// </summary>
public class ConflictException : ApplicationException
{
    public ConflictException() : base(ErrorMessageKeys.Conflict) { }

    public ConflictException(string resourceKey, params object[] parameters)
        : base(resourceKey, parameters) { }
}
