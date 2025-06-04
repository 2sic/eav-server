using System.Security.Cryptography.X509Certificates;
using System.Text;
using ToSic.Sys.Security.Encryption;

namespace ToSic.Sys.Utils.Tests.Security.Encryption;

public class Sha256Tests
{
    [Fact]
    public void Hash_WithNullInput_ThrowsArgumentNullException()
    {
        // Act & Assert
        Throws<ArgumentNullException>(() => Sha256.Hash(null!));
    }

    [Fact]
    public void Hash_WithEmptyString_ReturnsValidHash()
    {
        // Arrange
        var input = "";

        // Act
        var result = Sha256.Hash(input);

        // Assert
        NotNull(result);
        NotEmpty(result);
        // SHA256 hash of empty string should be consistent
        Equal("e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855", result);
    }

    [Fact]
    public void Hash_WithSameInput_ReturnsSameHash()
    {
        // Arrange
        var input = "test string";

        // Act
        var result1 = Sha256.Hash(input);
        var result2 = Sha256.Hash(input);

        // Assert
        Equal(result1, result2);
    }

    [Fact]
    public void Hash_WithDifferentInput_ReturnsDifferentHash()
    {
        // Arrange
        var input1 = "test string 1";
        var input2 = "test string 2";

        // Act
        var result1 = Sha256.Hash(input1);
        var result2 = Sha256.Hash(input2);

        // Assert
        NotEqual(result1, result2);
    }

    [Fact]
    public void Hash_WithKnownInput_ReturnsExpectedHash()
    {
        // Arrange
        var input = "hello world";

        // Act
        var result = Sha256.Hash(input);

        // Assert
        // SHA256 of "hello world" should be consistent
        Equal("b94d27b9934d3e08a52e52d7da7dabfac484efe37a5380ee9088f7ace2efcde9", result);
    }

    [Fact]
    public void Hash_WithUnicodeCharacters_ReturnsValidHash()
    {
        // Arrange
        var input = "Hello 世界 🌍";

        // Act
        var result = Sha256.Hash(input);

        // Assert
        NotNull(result);
        NotEmpty(result);
        // Hash should be 64 characters (32 bytes * 2 hex chars per byte)
        Equal(64, result.Length);
    }

    [Fact]
    public void Hash_WithLongString_ReturnsValidHash()
    {
        // Arrange
        var input = new string('a', 10000);

        // Act
        var result = Sha256.Hash(input);

        // Assert
        NotNull(result);
        NotEmpty(result);
        Equal(64, result.Length);
    }

    [Fact]
    public void Hash_ResultIsHexadecimal()
    {
        // Arrange
        var input = "test";

        // Act
        var result = Sha256.Hash(input);

        // Assert
        True(result.All(c => "0123456789abcdef".Contains(c)));
    }

    [Theory]
    [InlineData("")]
    [InlineData("a")]
    [InlineData("abc")]
    [InlineData("message digest")]
    [InlineData("abcdefghijklmnopqrstuvwxyz")]
    public void Hash_WithVariousInputs_ReturnsConsistentLength(string input)
    {
        // Act
        var result = Sha256.Hash(input);

        // Assert
        Equal(64, result.Length);
    }
    
    [Fact]
    public void SignBase64_WithNullCertificate_ThrowsArgumentNullException()
    {
        // Arrange
        var sha256 = new Sha256();
        var data = Encoding.UTF8.GetBytes("test data");

        // Act & Assert
        Throws<ArgumentNullException>(() => sha256.SignBase64(null!, data));
    }

    [Fact]
    public void SignBase64_WithInvalidCertificate_ThrowsFormatException()
    {
        // Arrange
        var sha256 = new Sha256();
        var data = Encoding.UTF8.GetBytes("test data");
        var invalidCert = "invalid-certificate-data";

        // Act & Assert
        Throws<FormatException>(() => sha256.SignBase64(invalidCert, data));    }

    [Fact]
    public void SignBase64_WithNullData_ThrowsArgumentNullException()
    {
        // Arrange
        var sha256 = new Sha256();
        var validCertBase64 = CreateSelfSignedCertificateBase64();

        // Act & Assert
        Throws<ArgumentNullException>(() => sha256.SignBase64(validCertBase64, null!));
    }

    [Fact]
    public void VerifyBase64_WithNullCertificate_ThrowsArgumentNullException()
    {
        // Arrange
        var sha256 = new Sha256();
        var data = Encoding.UTF8.GetBytes("test data");

        // Act & Assert
        Throws<ArgumentNullException>(() => sha256.VerifyBase64(null!, "signature", data));
    }

    [Fact]
    public void VerifyBase64_WithInvalidCertificate_ThrowsFormatException()
    {
        // Arrange
        var sha256 = new Sha256();
        var data = Encoding.UTF8.GetBytes("test data");
        var invalidCert = "invalid-certificate-data";

        // Act & Assert
        Throws<FormatException>(() => sha256.VerifyBase64(invalidCert, "signature", data));
    }

    [Fact]
    public void VerifyBase64_WithNullSignature_ThrowsArgumentNullException()
    {
        // Arrange
        var sha256 = new Sha256();
        var data = Encoding.UTF8.GetBytes("test data");
        var validCertBase64 = CreateSelfSignedCertificateBase64();

        // Act & Assert
        Throws<ArgumentNullException>(() => sha256.VerifyBase64(validCertBase64, null!, data));
    }

    [Fact]
    public void VerifyBase64_WithNullData_ThrowsArgumentNullException()
    {
        // Arrange
        var sha256 = new Sha256();
        var validCertBase64 = CreateSelfSignedCertificateBase64();
        var validSignature = Convert.ToBase64String(new byte[256]); // Valid base64 signature

        // Act & Assert
        Throws<ArgumentNullException>(() => sha256.VerifyBase64(validCertBase64, validSignature, null!));
    }

    [Fact]
    public void VerifyBase64_WithInvalidSignature_ThrowsFormatException()
    {
        // Arrange
        var sha256 = new Sha256();
        var data = Encoding.UTF8.GetBytes("test data");
        var validCertBase64 = CreateSelfSignedCertificateBase64();
        var invalidSignature = "invalid-signature";

        // Act & Assert
#if NETFRAMEWORK
        Throws<FormatException>(() => sha256.VerifyBase64(validCertBase64, invalidSignature, data));
#else
        Throws<System.Security.Cryptography.CryptographicException>(() => sha256.VerifyBase64(validCertBase64, invalidSignature, data));
#endif
    }

    /// <summary>
    /// Helper method to create a self-signed certificate for testing purposes
    /// </summary>
    private static string CreateSelfSignedCertificateBase64()
    {
        // This is a mock implementation for testing
        // In real tests, you would either use a test certificate or mock the certificate operations
        using var rsa = System.Security.Cryptography.RSA.Create(2048);
        var req = new System.Security.Cryptography.X509Certificates.CertificateRequest(
            "CN=Test", rsa, System.Security.Cryptography.HashAlgorithmName.SHA256,
            System.Security.Cryptography.RSASignaturePadding.Pkcs1);
        
        var cert = req.CreateSelfSigned(DateTimeOffset.Now, DateTimeOffset.Now.AddYears(1));
        return Convert.ToBase64String(cert.Export(X509ContentType.Pfx, ""));
    }
}
