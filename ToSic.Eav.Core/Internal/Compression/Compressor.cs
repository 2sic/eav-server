﻿using System.Text;
using ToSic.Eav.Internal.Features;

namespace ToSic.Eav.Internal.Compression;

public class Compressor(IEavFeaturesService features = null)
{
    private ICompressor _compressor = CompressorFactory(CompressorType.GZip);

    public Compressor InitCompressor(CompressorType compressorType)
    {
        _compressor = CompressorFactory(compressorType);
        return this;
    }

    private static ICompressor CompressorFactory(CompressorType compressorType) =>
        compressorType switch
        {
            CompressorType.NoCompression => null,
            CompressorType.Deflate => new DeflateCompressor(),
            CompressorType.GZip => new GZipCompressor(),
            CompressorType.Brotli => new BrotliCompressor(),
            _ => throw new ArgumentOutOfRangeException(nameof(compressorType), compressorType, null)
        };

    public bool IsEnabled => _isEnabled && _compressor != null;
    // ReSharper disable once ReplaceWithFieldKeyword
    private readonly bool _isEnabled = features?.IsEnabled(BuiltInFeatures.SqlCompressDataTimeline.NameId) ?? true;

    public byte[] Compress(string text) =>
        IsEnabled ? _compressor.CompressBytes(Encoding.UTF8.GetBytes(text)) : null;

    public string Decompress(byte[] bytes) =>
        IsEnabled ? Encoding.UTF8.GetString(_compressor.DecompressBytes(bytes)) : null;
}