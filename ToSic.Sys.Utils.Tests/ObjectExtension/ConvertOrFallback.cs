

namespace ToSic.Eav.Plumbing.ObjectExtension;


public partial class ConvertOrFallback: ConvertTestBase
{
    const string Fallback = "this-is-the-fallback";

    [Fact]
    public void StringToString()
    {
        ConvFbQuick<string>(null, null, null);
        ConvFbQuick(null, Fallback, Fallback);
        ConvFbQuick("test", Fallback, "test");
        ConvFbQuick("", Fallback, "");
    }



}