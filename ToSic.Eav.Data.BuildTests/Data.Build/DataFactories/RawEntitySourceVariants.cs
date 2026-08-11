using ToSic.Eav.Data.Build.DataFactories.MockData;
using ToSic.Eav.Data.Raw;
using ToSic.Eav.Data.Raw.Sys;

namespace ToSic.Eav.Data.Build.DataFactories;

/// <summary>
/// Test all known combinations of <see cref="IRawData"/>
/// </summary>
public class RawEntitySourceVariants
{
    [Fact]
    public void RawEntityIsConverted()
    {
        var x = new MockRawEntity();
        var y = x.GetRawFromConverterOrDirectCast(new());
        NotNull(y);
        Equal(MockRawEntity.DefaultId, y.Id);
    }

    [Fact]
    public void HasConverterIsConverted()
    {
        var x = new MockRawConvertible();
        var y = x.GetRawFromConverterOrDirectCast(new());
        NotNull(y);
        Equal(MockRawConvertible.DefaultId, y.Id);    // The ID is fixed to 92 by the HasDummy converter
    }

    [Fact]
    public void InvalidConverterThrows()
    {
        var x = new MockRawConvertibleInvalid();
        Throws<InvalidCastException>(() => x.GetRawFromConverterOrDirectCast(new()));
    }

    private IRawEntity AutoConverted => new MockRawAutoConvert().GetRawFromConverterOrDirectCast(new());

    [Fact]
    public void IRawEntityAutoConvert_Works()
        => NotNull(AutoConverted);

    [Fact]
    public void IRawEntityAutoConvert_HasName()
        => Equal(MockRawAutoConvert.NameDefault, AutoConverted.Values[nameof(MockRawAutoConvert.Name)]);

    [Fact]
    public void IRawEntityAutoConvert_HasOneValue()
        => Single(AutoConverted.Values);

    [Fact]
    public void IRawEntityAutoConvert_HasId()
        => Equal(MockRawAutoConvert.IdDefault, AutoConverted.Id);

    [Fact]
    public void IRawEntityAutoConvert_HasDefaultGuid()
        => Equal(Guid.Empty, AutoConverted.Guid);
}