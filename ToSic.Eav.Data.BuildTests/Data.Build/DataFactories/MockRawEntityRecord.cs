using ToSic.Eav.Data.Raw.Sys;

namespace ToSic.Eav.Data.Build.DataFactories;

internal record MockRawEntityRecord : IRawEntity
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
/// <param name="dataToProvideInConverter"></param>
internal record MockRawEntityProvidingConversion(IRawEntity dataToProvideInConverter) : MockRawEntityRecord, IGetRawConverter
{
    public IRawEntityConverter GetConverter() =>
        new ConvertToRawWithFactory<MockRawEntityProvidingConversion>((_, _) =>
            dataToProvideInConverter
        );
}