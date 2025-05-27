namespace ToSic.Eav.Security.Encryption;

[ShowApiWhenReleased(ShowApiMode.Never)]
public class EncryptionResult<T>
{
    /// <summary>
    /// The value as encrypted
    /// </summary>
    public required T Value { get; init; }

    /// <summary>
    /// The Initialization Vector which is needed to decrypt again
    /// </summary>
    public required T Iv { get; init; }

    /// <summary>
    /// The salt used to extend the password
    /// </summary>
    public required T Salt { get; init; }
}