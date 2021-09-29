using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ToSic.Eav.Core.Tests.PlumbingTests.ObjectExtensionTests
{
    public partial class ConvertOrFallback
    {

        [TestMethod]
        public void ObjectToInt()
        {
            // 0 should always default
            AllSameResult(new List<string>(), 27, 27);
            AllSameResult(new List<string>(), 42, 42);
        }

        [TestMethod]
        public void NullToInt()
        {
            // 0 should always default
            AllSameResult(null, 27, 27);
        }



        [TestMethod]
        public void StringToInt()
        {
            AllSameResult("0", 333, 0);
            AllSameResult("27", 333, 27);
            AllSameResult("-423", 333, -423);
            AllSameResult("", 42, 42);
            AllSameResult("nothing", 42, 42);
        }

        [TestMethod]
        public void StringToFloat()
        {
            AllSameResult("0", 333f, 0f);
            AllSameResult("27", 333f, 27f);
            AllSameResult("-423", 333f, -423f);
            AllSameResult("", 42f, 42f);
            AllSameResult("nothing", 42f, 42f);

            // now with correct decimals
            AllSameResult("0.1", 333f, 0.1f);
            AllSameResult("27.999", 333f, 27.999f);
            AllSameResult("-42.42", 333f, -42.42f);

            ConvWithAllOptions("0,1", 333f, false, 1f, 0.1f, 1f);
            ConvWithAllOptions("0,1", 333f, true, 1f, 0.1f, 1f);
            ConvWithAllOptions("27,999", 333f, false, 27999f, 27.999f, 27999f);
            ConvWithAllOptions("27,999", 333f, true, 27999f, 27.999f, 27999f);
            ConvWithAllOptions("-42,42", 333f, false, -4242f, -42.42f, -4242f);
            ConvWithAllOptions("-42,42", 333f, true, -4242f, -42.42f, -4242f);
        }

        [TestMethod]
        public void StringToDouble()
        {
            AllSameResult("0", 333d, 0d);
            AllSameResult("27", 333d, 27d);
            AllSameResult("-423", 333d, -423d);
            AllSameResult("", 42d, 42d);
            AllSameResult("nothing", 42d, 42d);

            // now with correct decimals
            AllSameResult("0.1", 333d, 0.1d);
            AllSameResult("27.999", 333d, 27.999d);
            AllSameResult("-42.42", 333d, -42.42d);

            ConvWithAllOptions("0,1", 333d, false, 1d, 0.1d, 1d);
            ConvWithAllOptions("0,1", 333d, true, 1d, 0.1d, 1d);
            ConvWithAllOptions("27,999", 333d, false, 27999d, 27.999d, 27999d);
            ConvWithAllOptions("27,999", 333d, true, 27999d, 27.999d, 27999d);
            ConvWithAllOptions("-42,42", 333d, false, -4242d, -42.42d, -4242d);
            ConvWithAllOptions("-42,42", 333d, true, -4242d, -42.42d, -4242d);
        }

        [TestMethod]
        public void StringToDecimal()
        {
            AllSameResult("0", 333m, 0m);
            AllSameResult("27", 333m, 27m);
            AllSameResult("-423", 333m, -423m);
            AllSameResult("", 42m, 42m);
            AllSameResult("nothing", 42m, 42m);

            // now with correct decimals
            AllSameResult("0.1", 333m, 0.1m);
            AllSameResult("27.999", 333m, 27.999m);
            AllSameResult("-42.42", 333m, -42.42m);

            ConvWithAllOptions("0,1", 333m, false, 1m, 0.1m, 1m);
            ConvWithAllOptions("0,1", 333m, true, 1m, 0.1m, 1m);
            ConvWithAllOptions("27,999", 333m, false, 27999m, 27.999m, 27999m);
            ConvWithAllOptions("27,999", 333m, true, 27999m, 27.999m, 27999m);
            ConvWithAllOptions("-42,42", 333m, false, -4242m, -42.42m, -4242m);
            ConvWithAllOptions("-42,42", 333m, true, -4242m, -42.42m, -4242m);
        }

    }
}
