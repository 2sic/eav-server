using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToSic.Eav.Compression
{
    public class BrotliCompressor
    {
        public static byte[] Compress(byte[] bytes)
        {
            using (var outputStream = new MemoryStream())
            {
                using (var compressStream = new /*BrotliStream*/DeflateStream(outputStream, CompressionLevel.Optimal))
                {
                    compressStream.Write(bytes, 0, bytes.Length);
                }

                return outputStream.ToArray();
            }
        }

        public static byte[] Decompress(byte[] bytes)
        {
            using (var inputStream = new MemoryStream(bytes))
            {
                using (var outputStream = new MemoryStream())
                {
                    using (var decompressStream = new /*BrotliStream*/DeflateStream(inputStream, CompressionMode.Decompress))
                    {
                        decompressStream.CopyTo(outputStream);
                    }

                    return outputStream.ToArray();
                }
            }
        }
    }
}
