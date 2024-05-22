using ToSic.Eav.Plumbing;
using static Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace ToSic.Eav.Core.Tests.PlumbingTests.ObjectExtensionTests;

public class ConvertTestBase 
{
    /// <summary>
    /// Convert Test Basic/Numeric - Test basic conversion and numeric conversion
    /// </summary>
    protected void ConvT<T>(object value, T exp, T expNumeric)
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
    protected void ConvT<T>(object value, T exp, T expNumeric, T expTruthy)
    {
        ConvT(value, exp, expNumeric);
        var resultTruthy = value.TestConvertOrDefault<T>(truthy: true);
        AreEqual(expTruthy, resultTruthy, $"Tested '{value}', expected tru: '{expTruthy}'");
        if (resultTruthy != null)
            AreEqual(typeof(T).UnboxIfNullable(), resultTruthy.GetType());
    }


    protected void ConvFbQuick<T>(object value, T fallback, T exp, bool doBasic = true, bool doOnDefault = true)
    {
        if (doBasic) 
            ConvFallback(value, fallback, false, exp, exp, exp);
        if (doOnDefault) 
            ConvFallback(value, fallback, true, exp, exp, exp);
    }


    protected void ConvFallback<T>(object value, T fallback, bool fallbackOnDefault, T exp, T expNumeric, T expTruthy)
    {
        var msg = $"V: {value}, F: {fallback}, fbOnDef: {fallbackOnDefault}, ";
        var result = value.TestConvertOrFallback(fallback, fallbackOnDefault: fallbackOnDefault);
        AreEqual(exp, result, $"Exp: {exp}; {msg}");
        result = value.TestConvertOrFallback(fallback, numeric: true, fallbackOnDefault: fallbackOnDefault);
        AreEqual(expNumeric, result, $"Num: {expNumeric}; {msg}");
        result = value.TestConvertOrFallback(fallback, truthy: true, fallbackOnDefault: fallbackOnDefault);
        AreEqual(expTruthy, result, $"Trt: {expTruthy}, {msg}");
    }

}