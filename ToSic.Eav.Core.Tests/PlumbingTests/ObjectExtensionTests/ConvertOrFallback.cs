using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ToSic.Eav.Core.Tests.PlumbingTests.ObjectExtensionTests
{
    [TestClass]
    public partial class ConvertOrFallback
    {
        const string Fallback = "this-is-the-fallback";

        [TestMethod]
        public void StringToString()
        {
            AllSameResult<string>(null, null, null);
            AllSameResult(null, Fallback, Fallback);
            AllSameResult("test", Fallback, "test");
            AllSameResult("", Fallback, "");
        }





        private void AllSameResult<T>(object value, T fallback, T exp, bool doBasic = true, bool doOnDefault = true)
        {
            if (doBasic) Basic(value, fallback, exp);
            if (doOnDefault) OnDefault(value, fallback, exp);
        }

        private void Basic<T>(object value, T fallback, T exp) => Basic(value, fallback, exp, exp, exp);

        private void Basic<T>(object value, T fallback, T exp, T expNumeric, T expTruthy) 
            => ConvWithAllOptions(value, fallback, false, exp, expNumeric, expTruthy);

        private void OnDefault<T>(object value, T fallback, T exp) => OnDefault(value, fallback, exp, exp, exp);
        private void OnDefault<T>(object value, T fallback, T exp, T expNumeric, T expTruthy) 
            => ConvWithAllOptions(value, fallback, true, exp, expNumeric, expTruthy);


        private void ConvWithAllOptions<T>(object value, T fallback, bool fallbackOnDefault, T exp, T expNumeric, T expTruthy)
        {
            var msg = $"V: {value}, F: {fallback}, fbOnDef: {fallbackOnDefault}, ";
            var result = value.TestConvertOrFallback(fallback, fallbackOnDefault: fallbackOnDefault);
            Assert.AreEqual(exp, result, $"Exp: {exp}; {msg}");
            result = value.TestConvertOrFallback(fallback, numeric: true, fallbackOnDefault: fallbackOnDefault);
            Assert.AreEqual(expNumeric, result, $"Num: {expNumeric}; {msg}");
            result = value.TestConvertOrFallback(fallback, truthy: true, fallbackOnDefault: fallbackOnDefault);
            Assert.AreEqual(expTruthy, result, $"Trt: {expTruthy}, {msg}");
        }
    }
}
