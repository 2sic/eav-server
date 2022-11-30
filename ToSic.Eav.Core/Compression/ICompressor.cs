namespace ToSic.Eav.Compression
{
    public interface ICompressor
    {
        byte[] CompressBytes(byte[] bytes);
        byte[] DecompressBytes(byte[] bytes);
    }
}