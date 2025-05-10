namespace ToSic.Eav.Security.Encryption;

[ShowApiWhenReleased(ShowApiMode.Never)]
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