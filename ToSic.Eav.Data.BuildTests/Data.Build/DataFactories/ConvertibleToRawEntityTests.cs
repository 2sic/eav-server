using ToSic.Eav.Data.Raw.Sys;

namespace ToSic.Eav.Data.Build.DataFactories;

/// <summary>
/// Test all known combinations of <see cref="IConvertibleToRawEntity"/>
/// </summary>
public class ConvertibleToRawEntityTests
{
    [Fact]
    public void RawEntityIsConverted()
    {
        IConvertibleToRawEntity x = new MockRawEntityRecord(new Dictionary<string, object?>());
        var y = x.GetRawEntity(new());
        NotNull(y);
        Equal(MockRawEntityRecord.DefaultId, y.Id);
    }

    [Fact]
    public void HasConverterIsConverted()
    {
        IConvertibleToRawEntity x = new MockRawConvertible();
        var y = x.GetRawEntity(new());
        NotNull(y);
        Equal(MockRawConvertible.DefaultId, y.Id);    // The ID is fixed to 92 by the HasDummy converter
    }

    [Fact]
    public void InvalidConverterThrows()
    {
        IConvertibleToRawEntity x = new MockRawConvertibleInvalid();
        Throws<InvalidOperationException>(() => x.GetRawEntity(new()));
    }
}