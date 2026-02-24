using System.Security.Cryptography;
using DreamSoft.Application.Common.Interfaces;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace DreamSoft.Infrastructure.Services.Common;

/// <summary>
/// Password hashing service using PBKDF2 with HMAC-SHA256.
/// Format: Base64(salt) + "." + Base64(hash) stored as a single string.
/// </summary>
public class PasswordHasherService : IPasswordHasher
{
    private const int SaltSize    = 16; // 128 bits
    private const int HashSize    = 32; // 256 bits
    private const int Iterations  = 100_000;

    public string HashPassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Password must not be empty.", nameof(password));

        // Generate a cryptographically random salt
        var salt = RandomNumberGenerator.GetBytes(SaltSize);

        var hash = KeyDerivation.Pbkdf2(
            password:   password,
            salt:       salt,
            prf:        KeyDerivationPrf.HMACSHA256,
            iterationCount: Iterations,
            numBytesRequested: HashSize);

        // Store as "Base64Salt.Base64Hash"
        return $"{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
    }

    public bool VerifyPassword(string password, string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(passwordHash))
            return false;

        var parts = passwordHash.Split('.');
        if (parts.Length != 2)
            return false;

        byte[] salt;
        byte[] expectedHash;

        try
        {
            salt         = Convert.FromBase64String(parts[0]);
            expectedHash = Convert.FromBase64String(parts[1]);
        }
        catch (FormatException)
        {
            return false;
        }

        var actualHash = KeyDerivation.Pbkdf2(
            password:   password,
            salt:       salt,
            prf:        KeyDerivationPrf.HMACSHA256,
            iterationCount: Iterations,
            numBytesRequested: HashSize);

        // Constant-time comparison to prevent timing attacks
        return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
    }
}
