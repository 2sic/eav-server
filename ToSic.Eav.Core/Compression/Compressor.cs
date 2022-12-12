using System;
using System.Text;
using ToSic.Eav.Configuration;

namespace ToSic.Eav.Compression
{
    public class Compressor
    {
        private readonly bool _featureEnabled;
        private ICompressor _compressor;

        public Compressor(IFeaturesInternal features = null)
        {
            // TODO: review with 2DM, because fallback is to "true" (used by unit testing)
            _featureEnabled = features?.IsEnabled(BuiltInFeatures.SqlCompressDataTimeline.NameId) ?? true;
            _compressor = CompressorFactory();
        }

        public Compressor InitCompressor(CompressorType compressorType)
        {
            _compressor = CompressorFactory(compressorType);
            return this;
        }

        private static ICompressor CompressorFactory(CompressorType compressorType = CompressorType.GZip)
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

        public bool IsEnabled => _featureEnabled && _compressor != null;

        public byte[] Compress(string text) => IsEnabled ? _compressor.CompressBytes(Encoding.UTF8.GetBytes(text)) : null;

        public string Decompress(byte[] bytes) => IsEnabled ? Encoding.UTF8.GetString(_compressor.DecompressBytes(bytes)) : null;
    }
}
