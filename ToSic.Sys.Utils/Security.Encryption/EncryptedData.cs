namespace ToSic.Sys.Security.Encryption;

/// <summary>
/// Special structure for encrypted form data - for example when submitting from Mobius.
/// </summary>
public class EncryptedData
{
    /// <summary>
    /// Version - in case compatibility would ever require different versions to be supported.
    /// Probably not relevant, since a new copy of 2sxc would include new encryption schemes.
    /// </summary>
    public int Version { get; init; } = 1;

    /// <summary>
    /// The underlying data which is encrypted.
    /// </summary>
    public required string Data { get; init; }

    /// <summary>
    /// The underlying data which is encrypted.
    /// </summary>
    public required string Key { get; init; }

    /// <summary>
    /// The underlying data which is encrypted.
    /// </summary>
    public required string Iv { get; init; }
}

/// <remarks>
/// If the data is encrypted, it would need to be required. But because we're test-deserializing this to do duck-checking, it cannot be required.
/// </remarks>
public class EncryptedDataRaw
{
    public int Version { get; init; } = 1;

    public string? Data { get; init; }

    public string? Key { get; init; }

    public string? Iv { get; init; }
}