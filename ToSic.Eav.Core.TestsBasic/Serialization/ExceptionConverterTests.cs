using System.Text.Json;

namespace ToSic.Eav.Serialization;

public class ExceptionConverterTests
{
    [Fact]
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

        Assert.Contains("42", json);
        Assert.Contains(paramName, json);
        Assert.Contains(iAmError, json);
        Assert.Contains(someOutOfRangeErrorOccurred, json);
        Assert.Contains("null", json);
    }
}