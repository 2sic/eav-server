using ToSic.Eav.Data.Raw.Sys;

namespace ToSic.Eav.Data.Build.DataFactories.MockData;

internal record MockRawConvertible : IRawEntityConvertible
{
    public const int DefaultId = 92;
    public IRawEntityConverter GetConverter() =>
        new RawEntityConverterFactory<MockRawConvertible>((_, _) =>
            new MockRawEntity
            {
                Id = DefaultId,
            }
        );
}