using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Text.Json;

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.Serialization.Tests;

[TestClass()]
public class ExceptionConverterTests
{
    [TestMethod()]
    // https://github.com/dotnet/runtime/issues/43026#issuecomment-949966701
    public void SerializeNestedExceptionsContainsNullsSuccess()
    {
        var converter = new ExceptionConverter<Exception>();

        var jsonOpts = new JsonSerializerOptions();
        jsonOpts.Converters.Add(converter);

        Exception ex;
        const string paramName = "MyParamName";
        const string someOutOfRangeErrorOccurred = "Some out of range error occurred";
        const string iAmError = "I AM ERROR";

        try
        {
                
            throw new ArgumentOutOfRangeException(paramName, 42, someOutOfRangeErrorOccurred);
        }
        catch (Exception innerException)
        {
            try
            {
                throw new(iAmError, innerException);
            }
            catch (Exception outer)
            {
                ex = outer;
            }
        }

        var json = JsonSerializer.Serialize(ex, jsonOpts);

        StringAssert.Contains(json, "42");
        StringAssert.Contains(json, paramName);
        StringAssert.Contains(json, iAmError);
        StringAssert.Contains(json, someOutOfRangeErrorOccurred);
        StringAssert.Contains(json, "null");
    }
}