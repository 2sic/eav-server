using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ToSic.Eav.Core.Tests.PlumbingTests.ObjectExtensionTests
{
    public partial class ChangeTypeTests
    {

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
