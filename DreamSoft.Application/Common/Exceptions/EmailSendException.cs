namespace DreamSoft.Application.Common.Exceptions;

/// <summary>
/// Exception thrown when email sending fails
/// </summary>
public class EmailSendException(params object[] parameters)
    : ApplicationException(ErrorMessageKeys.EmailSendFailed, parameters) { }
