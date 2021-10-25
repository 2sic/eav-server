using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ToSic.Eav.Core.Tests.PlumbingTests.ObjectExtensionTests
{
    [TestClass]
    public partial class ConvertOrFallback: ConvertTestBase
    {
        const string Fallback = "this-is-the-fallback";

        [TestMethod]
        public void StringToString()
        {
            ConvFbQuick<string>(null, null, null);
            ConvFbQuick(null, Fallback, Fallback);
            ConvFbQuick("test", Fallback, "test");
            ConvFbQuick("", Fallback, "");
        }



    }
}
