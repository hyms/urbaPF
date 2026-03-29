using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace UrbaPF.Infrastructure.Services;

public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string hash);
}

public class PasswordHasher : IPasswordHasher
{
    private const int Iterations = 100000;
    private const int SubkeyLength = 32;
    private const int SaltLength = 16;

    public string Hash(string password)
    {
        var salt = new byte[SaltLength];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(salt);

        var subkey = KeyDerivation.Pbkdf2(
            password,
            salt,
            KeyDerivationPrf.HMACSHA256,
            Iterations,
            SubkeyLength);

        var hashBytes = new byte[SaltLength + SubkeyLength];
        Buffer.BlockCopy(salt, 0, hashBytes, 0, SaltLength);
        Buffer.BlockCopy(subkey, 0, hashBytes, SaltLength, SubkeyLength);

        return Convert.ToBase64String(hashBytes);
    }

    public bool Verify(string password, string hash)
    {
        try
        {
            var hashBytes = Convert.FromBase64String(hash);

            if (hashBytes.Length != SaltLength + SubkeyLength)
                return false;

            var salt = new byte[SaltLength];
            Buffer.BlockCopy(hashBytes, 0, salt, 0, SaltLength);

            var storedSubkey = new byte[SubkeyLength];
            Buffer.BlockCopy(hashBytes, SaltLength, storedSubkey, 0, SubkeyLength);

            var computedSubkey = KeyDerivation.Pbkdf2(
                password,
                salt,
                KeyDerivationPrf.HMACSHA256,
                Iterations,
                SubkeyLength);

            return CryptographicOperations.FixedTimeEquals(computedSubkey, storedSubkey);
        }
        catch
        {
            return false;
        }
    }
}
