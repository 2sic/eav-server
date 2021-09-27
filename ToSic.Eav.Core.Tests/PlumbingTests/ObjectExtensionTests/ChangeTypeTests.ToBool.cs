using Microsoft.VisualStudio.TestTools.UnitTesting;
using static Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace ToSic.Eav.Core.Tests.PlumbingTests.ObjectExtensionTests
{
    public partial class ChangeTypeTests
    {

        [TestMethod]
        public void NumberToBool()
        {
            ConvT(null, false, false, false);
            ConvT(0, false, false, false);
            ConvT(0.1, true, true, true);
            ConvT(-0.1, true, true, true);
            ConvT(1, true, true, true);
            ConvT(-1, true, true, true);
            ConvT(2, true, true, true);
            ConvT(2.5, true, true, true);
            ConvT(-3.7, true, true, true);
        }

        [TestMethod]
        public void StringToBool()
        {
            ConvT(null, false, false, false);
            ConvT("", false, false, false);
            ConvT("0", false, false, false);
            ConvT("1", false, false, true);
            ConvT("-1", false, false, true);
            ConvT("5", false, false, true);
            ConvT("5.2", false, false, true);
            ConvT("true", true, true, true);
            ConvT("True", true, true, true);
            ConvT("TRUE", true, true, true);
            ConvT("false", false, false, false);
            ConvT("False", false, false, false);
            ConvT("FALSE", false, false, false);
        }

        [TestMethod]
        public void StringToBoolNull()
        {
            IsTrue(0.TestConvertOrDefault<bool?>().HasValue);
            IsTrue("true".TestConvertOrDefault<bool?>().HasValue);

            ConvT<bool?>(null, null, null, null);
            ConvT<bool?>("", null, null, null);
            ConvT<bool?>("0", null, null, false);
            ConvT<bool?>("1", null, null, true);
            ConvT<bool?>("-1", null, null, true);
            ConvT<bool?>("5", null, null, true);
            ConvT<bool?>("5.2", null, null, true);
            ConvT<bool?>("true", true, true, true);
            ConvT<bool?>("True", true, true, true);
            ConvT<bool?>("TRUE", true, true, true);
            ConvT<bool?>("false", false, false, false);
            ConvT<bool?>("False", false, false, false);
            ConvT<bool?>("FALSE", false, false, false);
        }
    }
}
