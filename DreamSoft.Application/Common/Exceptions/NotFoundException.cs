namespace DreamSoft.Application.Common.Exceptions;

/// <summary>
/// Exception thrown when a requested entity is not found
/// </summary>
public class NotFoundException : ApplicationException
{
    public NotFoundException(params object[] parameters) : base(ErrorMessageKeys.NotFound, parameters) { }

    public NotFoundException(string resourceKey, params object[] parameters)
        : base(resourceKey, parameters) { }
}
