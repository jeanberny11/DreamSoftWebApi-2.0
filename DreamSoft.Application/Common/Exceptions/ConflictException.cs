namespace DreamSoft.Application.Common.Exceptions;

/// <summary>
/// Exception thrown when there's a conflict with existing data (e.g., duplicate email)
/// </summary>
public class ConflictException : ApplicationException
{
    public ConflictException(string message)
        : base(message)
    {
    }

    public ConflictException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}