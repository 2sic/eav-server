using ToSic.Eav.Data.Raw.Sys;

namespace ToSic.Eav.Data.Build.DataFactories;

internal record MockRawConvertible : IRawEntityConvertible
{
    public const int DefaultId = 92;
    public IRawEntityConverter GetConverter() =>
        new RawEntityConverterFactory<MockRawConvertible>((_, _) =>
            new MockRawEntityRecord
            {
                Id = DefaultId,
            }
        );
}