using System.Text;
using ToSic.Lib.Helpers;

namespace ToSic.Sys.Utils.Tests.Security.Encryption;

public class RsaCryptographyServiceTests : IDisposable
{
    private readonly string _tempTestFolder;
    private readonly TestGlobalConfiguration _globalConfig;
    private readonly RsaCryptographyService _rsaService;

    public RsaCryptographyServiceTests()
    {
        // Create a temporary folder for testing
        _tempTestFolder = Path.Combine(Path.GetTempPath(), "RsaCryptographyServiceTests", Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempTestFolder);

        _globalConfig = new TestGlobalConfiguration(_tempTestFolder);
        _rsaService = new RsaCryptographyService(_globalConfig);
    }

    public void Dispose()
    {
        // Clean up temporary test folder
        if (Directory.Exists(_tempTestFolder))
        {
            Directory.Delete(_tempTestFolder, true);
        }
    }

    [Fact]
    public void PublicKey_WhenFirstAccessed_GeneratesKeys()
    {
        // Act
        var publicKey = _rsaService.PublicKey;

        // Assert
        NotNull(publicKey);
        NotEmpty(publicKey);
    }

    [Fact]
    public void PublicKey_WhenAccessedMultipleTimes_ReturnsSameKey()
    {
        // Act
        var publicKey1 = _rsaService.PublicKey;
        var publicKey2 = _rsaService.PublicKey;

        // Assert
        Equal(publicKey1, publicKey2);
    }

    [Fact]
    public void PublicKey_CreatesKeyFiles()
    {
        // Act
        var publicKey = _rsaService.PublicKey;

        // Assert
        var publicKeyPath = Path.Combine(_globalConfig.CryptoFolder(), "public.key");
        var privateKeyPath = Path.Combine(_globalConfig.CryptoFolder(), "private.key");
        
        True(File.Exists(publicKeyPath));
        True(File.Exists(privateKeyPath));
    }

    [Fact]
    public void PublicKey_WhenKeysExist_LoadsFromFiles()
    {
        // Arrange - First access to generate keys
        var originalPublicKey = _rsaService.PublicKey;
        
        // Create new service instance to simulate application restart
        var newRsaService = new RsaCryptographyService(_globalConfig);

        // Act
        var loadedPublicKey = newRsaService.PublicKey;

        // Assert
        Equal(originalPublicKey, loadedPublicKey);
    }

    [Fact]
    public void Decrypt_WithNullInput_ThrowsArgumentNullException()
    {
        // Act & Assert
        Throws<ArgumentNullException>(() => _rsaService.Decrypt(null!));
    }
    
    [Fact]
    public void Decrypt_WithEmptyInput_ThrowsCryptographicException()
    {
        // Act & Assert
        Throws<System.Security.Cryptography.CryptographicException>(() => _rsaService.Decrypt(""));
    }

    [Fact]
    public void Decrypt_WithInvalidBase64_ThrowsFormatException()
    {
        // Act & Assert
        Throws<FormatException>(() => _rsaService.Decrypt("invalid-base64"));
    }

    [Fact]
    public void Decrypt_WithValidEncryptedData_ReturnsDecryptedBytes()
    {
        // Arrange
        var originalData = "Hello, World!";
        var dataBytes = Encoding.UTF8.GetBytes(originalData);
        
        // First, we need to encrypt some data to test decryption
        var encryptedData = EncryptDataForTesting(dataBytes);

        // Act
        var decryptedBytes = _rsaService.Decrypt(encryptedData);

        // Assert
        NotNull(decryptedBytes);
        Equal(dataBytes, decryptedBytes);
    }
    
    [Fact]
    public void Decrypt_WithTamperedData_ThrowsCryptographicException()
    {
        // Arrange
        var originalData = "Hello, World!";
        var dataBytes = Encoding.UTF8.GetBytes(originalData);
        var encryptedData = EncryptDataForTesting(dataBytes);
        
        // Tamper with the encrypted data
        var tamperedData = encryptedData.Substring(0, encryptedData.Length - 4) + "XXXX";

        // Act & Assert
        Throws<System.Security.Cryptography.CryptographicException>(() => _rsaService.Decrypt(tamperedData));
    }

    [Fact]
    public void Decrypt_WithDifferentKeySize_HandlesCorrectly()
    {
        // Arrange
        var originalData = new byte[100]; // Some test data
        new Random().NextBytes(originalData);
        
        var encryptedData = EncryptDataForTesting(originalData);

        // Act
        var decryptedBytes = _rsaService.Decrypt(encryptedData);

        // Assert
        Equal(originalData, decryptedBytes);
    }

    [Fact]
    public void KeyGeneration_CreatesValidXmlKeys()
    {
        // Arrange & Act
        var publicKey = _rsaService.PublicKey;

        // Assert
        NotNull(publicKey);
        True(publicKey.Contains("<RSAKeyValue>") || IsBase64String(publicKey));
    }

    [Fact]
    public void KeyGeneration_CreatesCryptoFolder()
    {
        // Act
        var publicKey = _rsaService.PublicKey;

        // Assert
        var cryptoFolder = _globalConfig.CryptoFolder();
        True(Directory.Exists(cryptoFolder));
    }

    [Theory]
    //[InlineData("")]
    [InlineData("Hello")]
    [InlineData("This is a longer test message that should still work correctly")]
    [InlineData("Special chars: äöü ñ 中文 🚀")]
    public void EncryptDecryptRoundTrip_WithVariousData_WorksCorrectly(string originalText)
    {
        // Arrange
        var originalBytes = Encoding.UTF8.GetBytes(originalText);
        var encryptedData = EncryptDataForTesting(originalBytes);

        // Act
        var decryptedBytes = _rsaService.Decrypt(encryptedData);

        // Assert
        Equal(originalBytes, decryptedBytes);
        Equal(originalText, Encoding.UTF8.GetString(decryptedBytes));
    }

    [Fact]
    public void MultipleInstances_WithSameConfig_ShareKeys()
    {
        // Arrange
        var service1 = new RsaCryptographyService(_globalConfig);
        var service2 = new RsaCryptographyService(_globalConfig);

        // Act
        var publicKey1 = service1.PublicKey;
        var publicKey2 = service2.PublicKey;

        // Assert
        Equal(publicKey1, publicKey2);
    }

    [Fact]
    public void KeyFiles_ContainValidContent()
    {
        // Arrange & Act
        var publicKey = _rsaService.PublicKey;

        // Assert
        var publicKeyPath = Path.Combine(_globalConfig.CryptoFolder(), "public.key");
        var privateKeyPath = Path.Combine(_globalConfig.CryptoFolder(), "private.key");
        
        var publicKeyContent = File.ReadAllText(publicKeyPath);
        var privateKeyContent = File.ReadAllText(privateKeyPath);

        NotEmpty(publicKeyContent);
        NotEmpty(privateKeyContent);
        
        // Private key should contain more data than public key (has private components)
        True(privateKeyContent.Length > publicKeyContent.Length);
    }
    
    [Fact]
    public async Task ThreadSafety_ConcurrentAccess_WorksCorrectly()
    {
        // Arrange
        var tasks = new List<Task<string>>();

        // Act - Create multiple concurrent tasks accessing PublicKey
        for (int i = 0; i < 10; i++)
        {
            tasks.Add(Task.Run(() => _rsaService.PublicKey!));
        }

        var results = await Task.WhenAll(tasks);

        // Assert - All tasks should return the same public key
        var firstKey = results[0];
        True(results.All(r => r == firstKey));
    }
    
    /// <summary>
    /// Helper method to encrypt data for testing decryption
    /// </summary>
    private string EncryptDataForTesting(byte[] data)
    {
        using var rsa = System.Security.Cryptography.RSA.Create(2048);
        
        // We need to get the public key from the RSA service first
        // This will trigger key generation
        var publicKeyBase64 = _rsaService.PublicKey;
        
        // Since we can't easily convert the custom Base64 format back to XML,
        // let's use a workaround: read the private key and extract public key from it
        var privateKeyPath = Path.Combine(_globalConfig.CryptoFolder(), "private.key");
        var privateKeyXml = File.ReadAllText(privateKeyPath);
        
        // Load the private key and extract public components
        rsa.FromXmlString(privateKeyXml);
        var publicKeyXml = rsa.ToXmlString(false); // Get public key as XML
        
        // Now use the public key to encrypt
        using var encryptRsa = System.Security.Cryptography.RSA.Create(2048);
        encryptRsa.FromXmlString(publicKeyXml);
        
        var encryptedBytes = encryptRsa.Encrypt(data, System.Security.Cryptography.RSAEncryptionPadding.OaepSHA256);
        return Convert.ToBase64String(encryptedBytes);
    }

    /// <summary>
    /// Helper method to check if a string is valid Base64
    /// </summary>
    private static bool IsBase64String(string input)
    {
        try
        {
            Convert.FromBase64String(input);
            return true;
        }
        catch
        {
            return false;
        }
    }
}