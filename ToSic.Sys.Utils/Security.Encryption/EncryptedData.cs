namespace ToSic.Eav.Security.Encryption;

// TODO: @STV: Can we use more generic EncryptionResult
public class EncryptedData
{
    public int Version { get; init; } = 1;
    public required string Data { get; init; }
    public required string Key { get; init; }
    public required string Iv { get; init; }
}