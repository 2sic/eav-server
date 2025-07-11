﻿using System.IO.Compression;

namespace ToSic.Sys.Utils.Compression;

public class DeflateCompressor : ICompressor
{
    public byte[] CompressBytes(byte[] bytes)
    {
        using (var outputStream = new MemoryStream())
        {
            using (var compressStream = new DeflateStream(outputStream, CompressionLevel.Optimal))
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
                using (var decompressStream = new DeflateStream(inputStream, CompressionMode.Decompress))
                {
                    decompressStream.CopyTo(outputStream);
                }
                return outputStream.ToArray();
            }
        }
    }
}