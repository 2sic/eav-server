using System.Collections.Generic;
using System.Globalization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static System.Threading.Thread;

namespace ToSic.Eav.Core.Tests.PlumbingTests.ObjectExtensionTests
{
    public partial class ConvertOrFallback
    {

        [TestMethod]
        public void ObjectToInt()
        {
            // 0 should always default
            ConvFbQuick(new List<string>(), 27, 27);
            ConvFbQuick(new List<string>(), 42, 42);
        }

        [TestMethod]
        public void NullToInt()
        {
            // 0 should always default
            ConvFbQuick(null, 27, 27);
        }



        [TestMethod]
        public void StringToInt()
        {
            ConvFbQuick("0", 333, 0);
            ConvFbQuick("27", 333, 27);
            ConvFbQuick("-423", 333, -423);
            ConvFbQuick("", 42, 42);
            ConvFbQuick("nothing", 42, 42);
        }

        [TestMethod]
        public void StringToFloat()
        {
            ConvFbQuick("0", 333f, 0f);
            ConvFbQuick("27", 333f, 27f);
            ConvFbQuick("-423", 333f, -423f);
            ConvFbQuick("", 42f, 42f);
            ConvFbQuick("nothing", 42f, 42f);
        }

        [TestMethod]
        public void StringFractionToFloat()
        {
            // now with correct decimals
            ConvFbQuick("0.1", 333f, 0.1f);
            ConvFbQuick("27.999", 333f, 27.999f);
            ConvFbQuick("-42.42", 333f, -42.42f);

            ConvFallback("0,1", 333f, false, 1f, 0.1f, 1f);
            ConvFallback("0,1", 333f, true, 1f, 0.1f, 1f);
            ConvFallback("27,999", 333f, false, 27999f, 27.999f, 27999f);
            ConvFallback("27,999", 333f, true, 27999f, 27.999f, 27999f);
            ConvFallback("-42,42", 333f, false, -4242f, -42.42f, -4242f);
            ConvFallback("-42,42", 333f, true, -4242f, -42.42f, -4242f);
        }

        [TestMethod]
        public void StringFractionToFloatWrongCulture()
        {
            // Now change threading culture
            var current = CurrentThread.CurrentCulture;
            CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture("de-DE");

            // now with correct decimals
            ConvFbQuick("0.1", 333f, 0.1f);

            // undo to not affect other tests
            CurrentThread.CurrentCulture = current;
        }

        [TestMethod]
        public void StringToDouble()
        {
            ConvFbQuick("0", 333d, 0d);
            ConvFbQuick("27", 333d, 27d);
            ConvFbQuick("-423", 333d, -423d);
            ConvFbQuick("", 42d, 42d);
            ConvFbQuick("nothing", 42d, 42d);

            // now with correct decimals
            ConvFbQuick("0.1", 333d, 0.1d);
            ConvFbQuick("27.999", 333d, 27.999d);
            ConvFbQuick("-42.42", 333d, -42.42d);

            ConvFallback("0,1", 333d, false, 1d, 0.1d, 1d);
            ConvFallback("0,1", 333d, true, 1d, 0.1d, 1d);
            ConvFallback("27,999", 333d, false, 27999d, 27.999d, 27999d);
            ConvFallback("27,999", 333d, true, 27999d, 27.999d, 27999d);
            ConvFallback("-42,42", 333d, false, -4242d, -42.42d, -4242d);
            ConvFallback("-42,42", 333d, true, -4242d, -42.42d, -4242d);
        }

        [TestMethod]
        public void StringToDecimal()
        {
            ConvFbQuick("0", 333m, 0m);
            ConvFbQuick("27", 333m, 27m);
            ConvFbQuick("-423", 333m, -423m);
            ConvFbQuick("", 42m, 42m);
            ConvFbQuick("nothing", 42m, 42m);
        }

        [TestMethod]
        public void StringFractionsToDecimal()
        {
            // now with correct decimals
            ConvFbQuick("0.1", 333m, 0.1m);
            ConvFbQuick("27.999", 333m, 27.999m);
            ConvFbQuick("-42.42", 333m, -42.42m);

            ConvFallback("0,1", 333m, false, 1m, 0.1m, 1m);
            ConvFallback("0,1", 333m, true, 1m, 0.1m, 1m);
            ConvFallback("27,999", 333m, false, 27999m, 27.999m, 27999m);
            ConvFallback("27,999", 333m, true, 27999m, 27.999m, 27999m);
            ConvFallback("-42,42", 333m, false, -4242m, -42.42m, -4242m);
            ConvFallback("-42,42", 333m, true, -4242m, -42.42m, -4242m);
        }

    }
}
