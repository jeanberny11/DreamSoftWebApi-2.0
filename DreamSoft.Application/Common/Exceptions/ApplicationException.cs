namespace DreamSoft.Application.Common.Exceptions;

/// <summary>
/// Base exception class for application-level exceptions
/// Carries resource key and parameters for localization
/// </summary>
public class ApplicationException(string resourceKey, params object[] parameters) : Exception(resourceKey)
{
    /// <summary>
    /// Resource key for localized error message
    /// </summary>
    public string ResourceKey { get; } = resourceKey;

    /// <summary>
    /// Parameters to format the localized message
    /// </summary>
    public object[] Parameters { get; } = parameters;
}
