using System.IO.Compression;

namespace ToSic.Eav.Internal.Compression;

public class GZipCompressor : ICompressor
{
    public byte[] CompressBytes(byte[] bytes)
    {
        using (var outputStream = new MemoryStream())
        {
            using (var compressStream = new GZipStream(outputStream, CompressionLevel.Optimal))
            {
                compressStream.Write(bytes, 0, bytes.Length);
            }
            return outputStream.ToArray();
        }
    }

    public byte[] DecompressBytes(byte[] bytes)
    {
        using (var inputStream = new MemoryStream(bytes))
        {
            using (var outputStream = new MemoryStream())
            {
                using (var decompressStream = new GZipStream(inputStream, CompressionMode.Decompress))
                {
                    decompressStream.CopyTo(outputStream);
                }
                return outputStream.ToArray();
            }
        }
    }
}