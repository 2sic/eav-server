namespace ToSic.Eav.Security.Encryption
{
    // TODO: @STV: Can we use more generic EncryptionResult
    public class EncryptedData
    {
        public int Version { get; } = 1;
        public string Data { get; set; }
        public string Key { get; set; }
        public string Iv { get; set; }
    }
}
