using ToSic.Eav.Security.Encryption;

namespace ToSic.Eav.Core.Tests.Signature;

public class EncryptTests
{
    private const string TestMessage = "This is a test message";
    

    [Fact (Skip = "doesn't work at all yet, don't use")]
    public void EncryptAndDecryptUsingKeyPair()
    {

        var encryptor = new BasicEncryptionNotReadyForUse();
        var encrypted = encryptor.Encrypt(TestMessage);
        Assert.NotEqual(TestMessage, encrypted);

        var decrypted = encryptor.Decrypt(encrypted);
        Assert.Equal(TestMessage, decrypted);
    }
}