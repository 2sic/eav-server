using System;
using System.Globalization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace ToSic.Eav.Core.Tests.PlumbingTests.ObjectExtensionTests
{
    [TestClass]
    public class ConvertToString: ConvertTestBase
    {
        [TestMethod]
        [Ignore("ATM not ready, won't do what we would like but not sure if this is even relevant")]
        public void DateTimeToString()
        {
            ConvT(new DateTime(2021,09,29), "2021-09-29", "2021-09-29");
        }

        [TestMethod]
        public void StringToString()
        {
            AreEqual(null, (null as string).TestConvertOrDefault<string>());
            AreEqual("", "".TestConvertOrDefault<string>());
            AreEqual("5", "5".TestConvertOrDefault<string>());
        }


        [TestMethod]
        public void NumberToString()
        {
            ConvT(null, null as string, null);
            ConvT("", "", "");
            ConvT("5", "5", "5");
            ConvT(5.2, "5.2", "5.2");
            ConvT(5.299, "5.299", "5.299");
            ConvT(-5.2, "-5.2", "-5.2");

            // Now change threading culture
            System.Threading.Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture("de-DE");
            ConvT(5.2, "5.2", "5.2");
            ConvT(5.299, "5.299", "5.299");
            ConvT(-5.2, "-5.2", "-5.2");
        }

    }
}
