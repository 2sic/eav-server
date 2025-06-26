using System.IO.Compression;

namespace ToSic.Sys.Utils.Compression;

/// <summary>
/// Brotli Compressor - currently not working, so using GZip instead.
/// Reason is that it would only be natively supported in .net core.
/// </summary>
public class BrotliCompressorNotWorkingAtm : ICompressor
{
    public byte[] CompressBytes(byte[] bytes)
    {
        using (var outputStream = new MemoryStream())
        {
            using (var compressStream = new /*BrotliStream*/GZipStream(outputStream, CompressionLevel.Optimal))
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
                using (var decompressStream = new /*BrotliStream*/GZipStream(inputStream, CompressionMode.Decompress))
                {
                    decompressStream.CopyTo(outputStream);
                }
                return outputStream.ToArray();
            }
        }
    }
}