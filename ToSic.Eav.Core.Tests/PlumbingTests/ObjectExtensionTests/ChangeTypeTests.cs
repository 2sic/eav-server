using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.Plumbing;
using static Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace ToSic.Eav.Core.Tests.PlumbingTests.ObjectExtensionTests
{
    [TestClass]
    public partial class ChangeTypeTests
    {
        [TestMethod]
        public void StringToString()
        {
            AreEqual(null, (null as string).TestConvertOrDefault<string>());
            AreEqual("", "".TestConvertOrDefault<string>());
            AreEqual("5", "5".TestConvertOrDefault<string>());
        }





        /// <summary>
        /// Convert Test Basic/Numeric - Test basic conversion and numeric conversion
        /// </summary>
        private void ConvT<T>(object value, T exp, T expNumeric)
        {
            var resultDefault = value.TestConvertOrDefault<T>();
            AreEqual(exp, resultDefault, $"Tested '{value}', expected def: '{exp}'");
            if (resultDefault != null)
                AreEqual(typeof(T).UnboxIfNullable(), resultDefault.GetType());
            
            var resultNumeric = value.TestConvertOrDefault<T>(numeric: true);
            AreEqual(expNumeric, resultNumeric, $"Tested '{value}', expected num: '{expNumeric}'");
            if (resultNumeric != null)
                AreEqual(typeof(T).UnboxIfNullable(), resultNumeric.GetType());
        }

        /// <summary>
        /// Convert Test Basic/Numeric/Truthy - Test basic, numeric and truthy
        /// </summary>
        private void ConvT<T>(object value, T exp, T expNumeric, T expTruthy)
        {
            ConvT(value, exp, expNumeric);
            var resultTruthy = value.TestConvertOrDefault<T>(truthy: true);
            AreEqual(expTruthy, resultTruthy, $"Tested '{value}', expected tru: '{expTruthy}'");
            if (resultTruthy != null)
                AreEqual(typeof(T).UnboxIfNullable(), resultTruthy.GetType());
        }
    }
}
