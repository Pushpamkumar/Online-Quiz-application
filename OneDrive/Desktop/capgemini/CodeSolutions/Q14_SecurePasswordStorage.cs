using System;
using System.Security.Cryptography;
using System.Text;

/// <summary>
/// Secure Password Storage using PBKDF2 with Salt
/// Never store passwords in plain text!
/// </summary>
public class SecurePasswordUtility
{
    // PBKDF2 parameters
    private const int saltSize = 16;           // 16 bytes = 128 bits
    private const int hashSize = 20;           // 20 bytes = 160 bits
    private const int iterations = 10000;      // Number of iterations (higher = slower but more secure)

    /// <summary>
    /// Hash a password securely with a random salt
    /// Returns: salt + hash combined in a single string
    /// </summary>
    public string HashPassword(string password)
    {
        if (string.IsNullOrEmpty(password))
            throw new ArgumentException("Password cannot be empty");

        // Generate random salt
        byte[] salt = new byte[saltSize];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(salt);
        }

        // Use PBKDF2 to hash the password
        using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256))
        {
            byte[] hash = pbkdf2.GetBytes(hashSize);

            // Combine salt and hash into one string
            // Format: salt(Base64) + ":" + hash(Base64)
            string saltBase64 = Convert.ToBase64String(salt);
            string hashBase64 = Convert.ToBase64String(hash);

            return $"{saltBase64}:{hashBase64}";
        }
    }

    /// <summary>
    /// Verify if a password matches the stored hash
    /// Returns: true if password is correct, false otherwise
    /// </summary>
    public bool VerifyPassword(string password, string storedHash)
    {
        if (string.IsNullOrEmpty(password))
            throw new ArgumentException("Password cannot be empty");

        if (string.IsNullOrEmpty(storedHash))
            throw new ArgumentException("Stored hash cannot be empty");

        try
        {
            // Split the stored hash to get salt and hash
            string[] hashParts = storedHash.Split(':');
            if (hashParts.Length != 2)
                return false;

            byte[] salt = Convert.FromBase64String(hashParts[0]);
            byte[] storedHashBytes = Convert.FromBase64String(hashParts[1]);

            // Hash the provided password with the same salt
            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256))
            {
                byte[] computedHash = pbkdf2.GetBytes(hashSize);

                // Compare computed hash with stored hash
                // Use constant-time comparison to prevent timing attacks
                return BytesEqual(computedHash, storedHashBytes);
            }
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Constant-time byte array comparison (prevents timing attacks)
    /// </summary>
    private bool BytesEqual(byte[] a, byte[] b)
    {
        if (a.Length != b.Length)
            return false;

        int result = 0;
        for (int i = 0; i < a.Length; i++)
        {
            result |= a[i] ^ b[i];
        }

        return result == 0;
    }
}

// ============= USAGE EXAMPLE =============
class PasswordSecurityDemo
{
    static void Main()
    {
        var passwordManager = new SecurePasswordUtility();

        Console.WriteLine("=== Secure Password Storage Demo ===\n");

        // User registers with password
        string userPassword = "MySecure@Password123";
        Console.WriteLine($"User password: {userPassword}");

        // Store the hash (not the password!)
        string storedHash = passwordManager.HashPassword(userPassword);
        Console.WriteLine($"\nStored hash (never store plain password):");
        Console.WriteLine($"{storedHash}\n");

        // User tries to log in with correct password
        Console.WriteLine("--- Login Attempt 1: Correct Password ---");
        bool isValid1 = passwordManager.VerifyPassword("MySecure@Password123", storedHash);
        Console.WriteLine($"Password 'MySecure@Password123' is valid: {isValid1}");

        // User tries with wrong password
        Console.WriteLine("\n--- Login Attempt 2: Wrong Password ---");
        bool isValid2 = passwordManager.VerifyPassword("WrongPassword", storedHash);
        Console.WriteLine($"Password 'WrongPassword' is valid: {isValid2}");

        // User tries with case-sensitive variation
        Console.WriteLine("\n--- Login Attempt 3: Wrong Case ---");
        bool isValid3 = passwordManager.VerifyPassword("mysecure@password123", storedHash);
        Console.WriteLine($"Password 'mysecure@password123' is valid: {isValid3}");

        // Demonstrate multiple users with same password have different hashes
        Console.WriteLine("\n--- Two Users, Same Password, Different Hashes ---");
        string samePassword = "Password123";
        string hash1 = passwordManager.HashPassword(samePassword);
        string hash2 = passwordManager.HashPassword(samePassword);

        Console.WriteLine($"Hash 1: {hash1}");
        Console.WriteLine($"Hash 2: {hash2}");
        Console.WriteLine($"Same? {hash1 == hash2}");
        Console.WriteLine("(Different hashes because each has a random salt!)");

        // But both verify correctly
        Console.WriteLine($"\nBoth verify correctly:");
        Console.WriteLine($"Hash1 matches password: {passwordManager.VerifyPassword(samePassword, hash1)}");
        Console.WriteLine($"Hash2 matches password: {passwordManager.VerifyPassword(samePassword, hash2)}");
    }
}
