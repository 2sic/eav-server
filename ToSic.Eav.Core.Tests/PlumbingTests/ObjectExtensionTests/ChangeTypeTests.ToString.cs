using System;
using System.Globalization;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ToSic.Eav.Core.Tests.PlumbingTests.ObjectExtensionTests
{
    public partial class ChangeTypeTests
    {
        [TestMethod]
        [Ignore("ATM not ready, won't do what we would like but not sure if this is even relevant")]
        public void DateTimeToString()
        {
            ConvT(new DateTime(2021,09,29), "2021-09-29", "2021-09-29");
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

            // todo: now change threading culture
            var current = System.Globalization.CultureInfo.CurrentCulture; //.Thread.CurrentThread.CurrentCulture;
            System.Threading.Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture("de-DE");
            ConvT(5.2, "5.2", "5.2");
            ConvT(5.299, "5.299", "5.299");
            ConvT(-5.2, "-5.2", "-5.2");
        }

    }
}
