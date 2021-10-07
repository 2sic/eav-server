using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ToSic.Eav.Core.Tests.PlumbingTests.ObjectExtensionTests
{
    [TestClass]
    public class ConvertToNumber: ConvertTestBase
    {


        [TestMethod]
        public void StringToInt()
        {
            ConvT(null, 0, 0);
            ConvT("", 0, 0);
            ConvT("5", 5, 5);
            ConvT("5.2", 0, 5);
            ConvT("5.4", 0, 5);
            ConvT("5.5", 0, 6);
            ConvT("5.9", 0, 6);
            ConvT("   5.9", 0, 6);
            ConvT("5.9  ", 0, 6);
            ConvT("   5.9  ", 0, 6);
        }

        [TestMethod]
        public void StringToIntNull()
        {
            ConvT<int?>(null, null, null);
            ConvT<int?>("", null, null);
            ConvT<int?>("5", 5, 5);
            ConvT<int?>("5.2", null, 5);
            ConvT<int?>("5.4", null, 5);
            ConvT<int?>("5.5", null, 6);
            ConvT<int?>("5.9", null, 6);
        }

        [TestMethod]
        public void StringToFloat()
        {
            ConvT(null, 0f, 0f);
            ConvT("", 0f, 0f);
            ConvT("5", 5f, 5f);
            ConvT("5.2", 5.2f, 5.2f);
            ConvT("5.9", 5.9f, 5.9f);
            ConvT("-1", -1f, -1f);
            ConvT("-99.7", -99.7f, -99.7f);
        }

        [TestMethod]
        public void StringToFloatNull()
        {
            ConvT<float?>(null, null, null);
            ConvT<float?>("", null, null);
            ConvT<float?>("5", 5f, 5f);
            ConvT<float?>("5.2", 5.2f, 5.2f);
            ConvT<float?>("5.9", 5.9f, 5.9f);
            ConvT<float?>("-1", -1f, -1f);
            ConvT<float?>("-99.7", -99.7f, -99.7f);
        }
    }
}
