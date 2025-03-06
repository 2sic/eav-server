namespace ToSic.Eav.Plumbing.ObjectExtension;

public partial class ConvertOrFallback
{
    [Fact]
    public void NumberToBoolFallback()
    {
        // 0 should false
        ConvFbQuick(0, true, false);
        ConvFbQuick(0, false, false);

        // All other numbers should true
        ConvFbQuick(1, true, true);
        ConvFbQuick(1, false, true);
        ConvFbQuick(2, true, true);
        ConvFbQuick(2, false, true);
        ConvFbQuick(-1, true, true);
        ConvFbQuick(-1, false, true);
        ConvFbQuick(27.3, true, true);
        ConvFbQuick(27.3, false, true);
        ConvFbQuick(0.1, true, true);
        ConvFbQuick(0.1, false, true);
    }

    [Fact]
    public void NumberNullableToBoolFallback()
    {
        // 0 should false
        ConvFbQuick(new int?(), true, true);
        ConvFbQuick(new int?(), false, false);
        ConvFbQuick(new int?(0), true, false);
        ConvFbQuick(new int?(0), false, false);

        // All other numbers should true
        ConvFbQuick(new int?(1), true, true);
        ConvFbQuick(new int?(1), false, true);
        ConvFbQuick(new int?(2), true, true);
        ConvFbQuick(new int?(2), false, true);
        ConvFbQuick(new int?(-1), true, true);
        ConvFbQuick(new int?(-1), false, true);
        ConvFbQuick(new float?(27.3f), true, true);
        ConvFbQuick(new float?(27.3f), false, true);
        ConvFbQuick(new double?(0.1), true, true);
        ConvFbQuick(new double?(0.1), false, true);
    }

    [Fact]
    public void ObjectToBoolFallback()
    {
        // all objects should default
        ConvFbQuick(new List<string>(), true, true);
        ConvFbQuick(new List<string>(), false, false);
    }
    [Fact]
    public void StringToBoolFallback()
    {
        // Nulls should always false
        ConvFbQuick(null, true, true);
        ConvFbQuick(null, false, false);

        // Strange strings should always false
        ConvFbQuick("", false, false);
        ConvFbQuick("null", false, false);
        ConvFbQuick("random", false, false);

        // True strings should true
        ConvFbQuick("true", false, true);
        ConvFbQuick("TRUE", false, true);
        ConvFbQuick(" TRUE", false, true);
        ConvFbQuick("  TRUE  ", false, true);

        // False strings should always false
        ConvFbQuick("false", true, false);
        ConvFbQuick("False", true, false);
        ConvFbQuick("FALSE", true, false);
        ConvFbQuick(" false", true, false);
        ConvFbQuick("  false  ", true, false);
    }

}