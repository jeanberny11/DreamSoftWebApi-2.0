namespace DreamSoft.Application.Common.Interfaces;

/// <summary>
/// Service for password hashing and verification
/// </summary>
public interface IPasswordHasher
{
    /// <summary>
    /// Hashes a password using PBKDF2
    /// </summary>
    string HashPassword(string password);

    /// <summary>
    /// Verifies a password against a hash
    /// </summary>
    bool VerifyPassword(string password, string passwordHash);
}