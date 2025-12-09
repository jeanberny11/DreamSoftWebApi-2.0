namespace DreamSoft.Application.Common.Exceptions;

/// <summary>
/// Exception thrown when there's a EmailSendException with existing data (e.g., duplicate email)
/// </summary>
public class EmailSendException : ApplicationException
{
    public EmailSendException(string message)
        : base(message)
    {
    }

    public EmailSendException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}