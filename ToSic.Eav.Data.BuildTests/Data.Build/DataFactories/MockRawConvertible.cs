using ToSic.Eav.Data.Raw.Sys;

namespace ToSic.Eav.Data.Build.DataFactories;

internal record MockRawConvertible : IGetRawConverter
{
    public const int DefaultId = 92;
    public IRawEntityConverter GetConverter() =>
        new ConvertToRawWithFactory<MockRawConvertible>((_, _) =>
            new MockRawEntityRecord
            {
                Id = DefaultId,
            }
        );
}