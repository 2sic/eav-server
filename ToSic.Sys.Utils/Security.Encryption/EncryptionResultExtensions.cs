namespace ToSic.Eav.Security.Encryption;

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