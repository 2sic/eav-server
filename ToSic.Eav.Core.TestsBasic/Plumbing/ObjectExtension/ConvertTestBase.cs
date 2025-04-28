namespace ToSic.Eav.Plumbing.ObjectExtension;

public class ConvertTestBase 
{
    /// <summary>
    /// Convert Test Basic/Numeric - Test basic conversion and numeric conversion
    /// </summary>
    protected void ConvT<T>(object value, T exp, T expNumeric)
    {
        var resultDefault = value.ConvertOrDefaultTac<T>();
        Assert.Equal(exp, resultDefault); //, $"Tested '{value}', expected def: '{exp}'");
        if (resultDefault != null)
            Assert.Equal(typeof(T).UnboxIfNullable(), resultDefault.GetType());

        var resultNumeric = value.ConvertOrDefaultTac<T>(numeric: true);
        Assert.Equal(expNumeric, resultNumeric); //, $"Tested '{value}', expected num: '{expNumeric}'");
        if (resultNumeric != null)
            Assert.Equal(typeof(T).UnboxIfNullable(), resultNumeric.GetType());
    }

    /// <summary>
    /// Convert Test Basic/Numeric/Truthy - Test basic, numeric and truthy
    /// </summary>
    protected void ConvT<T>(object value, T exp, T expNumeric, T expTruthy)
    {
        ConvT(value, exp, expNumeric);
        var resultTruthy = value.ConvertOrDefaultTac<T>(truthy: true);
        Assert.Equal(expTruthy, resultTruthy); //, $"Tested '{value}', expected tru: '{expTruthy}'");
        if (resultTruthy != null)
            Assert.Equal(typeof(T).UnboxIfNullable(), resultTruthy.GetType());
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
        var result = value.ConvertOrFallbackTac(fallback, fallbackOnDefault: fallbackOnDefault);
        Assert.Equal(exp, result); //, $"Exp: {exp}; {msg}");
        result = value.ConvertOrFallbackTac(fallback, numeric: true, fallbackOnDefault: fallbackOnDefault);
        Assert.Equal(expNumeric, result); //, $"Num: {expNumeric}; {msg}");
        result = value.ConvertOrFallbackTac(fallback, truthy: true, fallbackOnDefault: fallbackOnDefault);
        Assert.Equal(expTruthy, result); //, $"Trt: {expTruthy}, {msg}");
    }

}