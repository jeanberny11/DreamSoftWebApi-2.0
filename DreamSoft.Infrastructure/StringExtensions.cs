namespace DreamSoft.Infrastructure;

/// <summary>
/// General-purpose string extensions used across the Infrastructure layer.
/// </summary>
internal static class StringExtensions
{
    /// <summary>
    /// Returns null if the string is null or whitespace, otherwise returns the original value.
    /// Useful for ?? fallback chains where an empty string should be treated the same as null.
    /// </summary>
    public static string? NullIfEmpty(this string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value;
}
