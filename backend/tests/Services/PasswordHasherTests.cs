using FluentAssertions;
using UrbaPF.Infrastructure.Services;

namespace UrbaPF.Tests.Services;

public class PasswordHasherTests
{
    private readonly PasswordHasher _hasher;

    public PasswordHasherTests()
    {
        _hasher = new PasswordHasher();
    }

    [Test]
    public void Hash_WithValidPassword_ReturnsNonEmptyHash()
    {
        var result = _hasher.Hash("password123");

        result.Should().NotBeNullOrEmpty();
    }

    [Test]
    public void Hash_WithSamePassword_ReturnsDifferentHashes()
    {
        var hash1 = _hasher.Hash("password123");
        var hash2 = _hasher.Hash("password123");

        hash1.Should().NotBe(hash2);
    }

    [Test]
    public void Verify_WithCorrectPassword_ReturnsTrue()
    {
        var password = "password123";
        var hash = _hasher.Hash(password);

        var result = _hasher.Verify(password, hash);

        result.Should().BeTrue();
    }

    [Test]
    public void Verify_WithIncorrectPassword_ReturnsFalse()
    {
        var hash = _hasher.Hash("correctpassword");

        var result = _hasher.Verify("wrongpassword", hash);

        result.Should().BeFalse();
    }

    [Test]
    public void Verify_WithEmptyPassword_ReturnsFalse()
    {
        var hash = _hasher.Hash("password123");

        var result = _hasher.Verify("", hash);

        result.Should().BeFalse();
    }

    [Test]
    public void Verify_WithInvalidHash_ReturnsFalse()
    {
        var result = _hasher.Verify("password", "invalidhash");

        result.Should().BeFalse();
    }

    [Test]
    public void Verify_WithTruncatedHash_ReturnsFalse()
    {
        var hash = _hasher.Hash("password123");
        var truncatedHash = hash.Substring(0, hash.Length / 2);

        var result = _hasher.Verify("password123", truncatedHash);

        result.Should().BeFalse();
    }

    [Test]
    public void Hash_ProducesBase64String()
    {
        var hash = _hasher.Hash("password");

        var action = () => Convert.FromBase64String(hash);

        action.Should().NotThrow();
    }

    [Test]
    public void Hash_Produces48ByteHash()
    {
        var hash = _hasher.Hash("password");
        var hashBytes = Convert.FromBase64String(hash);

        hashBytes.Length.Should().Be(48);
    }
}
