using System;
using System.Text;

namespace ToSic.Eav.Compression
{
    public class Compressor
    {
        public Compressor()
        {
            Enabled = true; // TODO: implement feature
        }

        public Compressor InitCompressor(CompressorType compressorType)
        {
            _compressor = CompressorFactory(compressorType);
            return this;
        }

        private static ICompressor CompressorFactory(CompressorType compressorType = CompressorType.Deflate)
        {
            switch (compressorType)
            {
                case CompressorType.NoCompression:
                    return null;

                case CompressorType.Deflate:
                    return new DeflateCompressor();

                case CompressorType.GZip:
                    return new GZipCompressor();

                case CompressorType.Brotli:
                    return new BrotliCompressor();

                default:
                    throw new ArgumentOutOfRangeException(nameof(compressorType), compressorType, null);
            }
        }

        private ICompressor GetCompressor => _compressor ?? (_compressor = CompressorFactory());
        private ICompressor _compressor;

        public bool IsEnabled => _compressor != null && Enabled;
        private bool Enabled { get; } = false;

        public byte[] Compress(string text) => IsEnabled ? GetCompressor.CompressBytes(Encoding.UTF8.GetBytes(text)) : null;

        public string Decompress(byte[] bytes) => IsEnabled ? Encoding.UTF8.GetString(GetCompressor.DecompressBytes(bytes)) : null;
    }
}
