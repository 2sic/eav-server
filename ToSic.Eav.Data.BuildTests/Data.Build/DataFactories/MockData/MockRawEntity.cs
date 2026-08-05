using ToSic.Eav.Data.Raw;
using ToSic.Eav.Data.Raw.Sys;

namespace ToSic.Eav.Data.Build.DataFactories.MockData;

internal record MockRawEntity : IRawEntity
{
    public const int DefaultId = 42;

    public int Id { get; init; } = DefaultId;
    public Guid Guid { get; init; } = Guid.NewGuid();
    public DateTime Created { get; init; } = DateTime.Now;
    public DateTime Modified { get; init; } = DateTime.Now;
    public IDictionary<string, object?> Values { get; init; } = new Dictionary<string, object?>();
}

/// <summary>
/// This is a RawEntity, but if things work correctly, it will not provide its own data, but instead the test-raw-entity in the constructor.
/// </summary>
/// <param name="_dataToProvideInConverter"></param>
// ReSharper disable once InconsistentNaming
internal record MockRawEntityProvidingConversion(IRawEntity _dataToProvideInConverter) : MockRawEntity, IRawEntityConvertible
{
    public IRawEntityConverter GetConverter() =>
        new RawEntityConverterFactory<MockRawEntityProvidingConversion>((_, _) =>
            _dataToProvideInConverter
        );
}

internal record MockRawAutoConvert : IRawEntityAutoConvert
{
    public int Id => IdDefault;
    public const int IdDefault = 7;
    public string Name => NameDefault;
    public const string NameDefault = "MockRawAutoConvert";
}