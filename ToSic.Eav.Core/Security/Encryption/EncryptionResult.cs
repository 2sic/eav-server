using System;

namespace ToSic.Eav.Security.Encryption;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class EncryptionResult<T>
{
    /// <summary>
    /// The value as encrypted
    /// </summary>
    public T Value { get; set; }

    /// <summary>
    /// The Initialization Vector which is needed to decrypt again
    /// </summary>
    public T Iv { get; set; }

    /// <summary>
    /// The salt used to extend the password
    /// </summary>
    public T Salt { get; set; }
}

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public static class EncryptionResultExtensions
{
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public static EncryptionResult<string> ToBase64(this EncryptionResult<byte[]> original)
    {
        return new()
        {
            Value = Convert.ToBase64String(original.Value),
            Iv = Convert.ToBase64String(original.Iv),
            Salt = Convert.ToBase64String(original.Salt),
        };
    }
}