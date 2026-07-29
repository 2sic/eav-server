using ToSic.Eav.Data.Build.DataFactories.MockData;
using ToSic.Eav.Data.Raw.Sys;

namespace ToSic.Eav.Data.Build.DataFactories;

/// <summary>
/// Test all known combinations of <see cref="IRawEntitySource"/>
/// </summary>
public class RawEntitySourceVariants
{
    [Fact]
    public void RawEntityIsConverted()
    {
        IRawEntitySource x = new MockRawEntity();
        var y = x.GetRawFromConverterOrDirectCast(new());
        NotNull(y);
        Equal(MockRawEntity.DefaultId, y.Id);
    }

    [Fact]
    public void HasConverterIsConverted()
    {
        IRawEntitySource x = new MockRawConvertible();
        var y = x.GetRawFromConverterOrDirectCast(new());
        NotNull(y);
        Equal(MockRawConvertible.DefaultId, y.Id);    // The ID is fixed to 92 by the HasDummy converter
    }

    [Fact]
    public void InvalidConverterThrows()
    {
        IRawEntitySource x = new MockRawConvertibleInvalid();
        Throws<InvalidCastException>(() => x.GetRawFromConverterOrDirectCast(new()));
    }
}