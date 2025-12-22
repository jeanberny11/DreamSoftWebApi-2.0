namespace DreamSoft.Application.Common.Exceptions;

/// <summary>
/// Exception thrown when email sending fails
/// </summary>
public class EmailSendException(string resourceKey, params object[] parameters) : ApplicationException(resourceKey, parameters)
{
}
