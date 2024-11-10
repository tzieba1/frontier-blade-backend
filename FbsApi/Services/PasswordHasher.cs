using System.Security.Cryptography;
using FbsApi.Services.Interfaces;

namespace FbsApi.Services
{
    public class PasswordHasher : IPasswordHasher
    {
        private const int SaltSize = 16; // 128-bit
        private const int KeySize = 32;  // 256-bit
        private const int DefaultIterations = 100_000;
        private const char Delimiter = '.';

        public string HashPassword(string password, int? iterations = null)
        {
            iterations ??= DefaultIterations;
            using var algorithm = new Rfc2898DeriveBytes(
                password,
                SaltSize,
                iterations.Value,
                HashAlgorithmName.SHA256);

            var salt = algorithm.Salt;
            var key = algorithm.GetBytes(KeySize);

            // Format: {iterations}.{salt}.{hash}
            return string.Join(Delimiter, iterations,
                Convert.ToBase64String(salt),
                Convert.ToBase64String(key));
        }

        public bool VerifyPassword(string hashedPassword, string providedPassword)
        {
            var parts = hashedPassword.Split(Delimiter);
            if (parts.Length != 3)
            {
                // Invalid hash format
                return false;
            }

            if (!int.TryParse(parts[0], out int iterations))
            {
                // Invalid iteration count
                return false;
            }

            var salt = Convert.FromBase64String(parts[1]);
            var key = Convert.FromBase64String(parts[2]);

            using var algorithm = new Rfc2898DeriveBytes(
                providedPassword,
                salt,
                iterations,
                HashAlgorithmName.SHA256);

            var keyToCheck = algorithm.GetBytes(KeySize);

            // Use constant-time comparison
            return keyToCheck.SequenceEqual(key);
        }
    }
}