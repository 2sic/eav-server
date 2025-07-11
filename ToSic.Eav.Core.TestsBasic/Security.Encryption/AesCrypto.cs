﻿using System.Diagnostics;
using ToSic.Sys.Security.Encryption;

namespace ToSic.Eav.Core.Tests.Signature;

public class AesCrypto
{
    private AesCryptographyService GetAes() => new(new()) { Debug = true };

    private const string TestMessage = "This is a test message";
    private const string DummyPassword = "dummy-password";
    private const string PreviousEncryptionSha256 = "L3pBTTJ9+Ow1aRRWX4Flykh3UEfO7/XffcEuJPVKABg=";

    [Fact]
    public void TestBasicAesCrypto()
    {
        var crypto = GetAes();
        var encrypted = crypto.EncryptToBase64(TestMessage, new() { Password = DummyPassword });
        Trace.WriteLine($"encrypted:'{encrypted.Value}'; IV:'{encrypted.Iv}'");
        Trace.WriteLine(crypto.Log.Dump());
        NotEqual(TestMessage, encrypted.Value);
        NotEqual(PreviousEncryptionSha256, encrypted.Value); //, "each encryption should give a different result - but that's not implemented yet");

        // reset crypto to get a fresh log
        crypto = GetAes();
        var decrypted = crypto.DecryptFromBase64(encrypted.Value, new()
        {
            Password = DummyPassword,
            InitializationVector64 = encrypted.Iv
        });
        Trace.WriteLine($"decrypted:{decrypted}");
        Trace.WriteLine(crypto.Log.Dump());
        Equal(TestMessage, decrypted);
    }

}