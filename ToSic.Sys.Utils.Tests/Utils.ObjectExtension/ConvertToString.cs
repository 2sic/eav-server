using System.Globalization;

namespace ToSic.Sys.Utils.ObjectExtension;


public class ConvertToString: ConvertTestBase
{
    [Fact (Skip = "ATM not ready, won't do what we would like but not sure if this is even relevant")]
    public void DateTimeToString()
    {
        RunConvTest(new DateTime(2021,09,29), "2021-09-29", "2021-09-29");
    }

    [Fact]
    public void StringToString()
    {
        Equal(null, (null as string).ConvertOrDefaultTac<string>());
        Equal("", "".ConvertOrDefaultTac<string>());
        Equal("5", "5".ConvertOrDefaultTac<string>());
    }


    [Fact]
    public void NumberToString()
    {
        RunConvTest(null, null as string, null);
        RunConvTest("", "", "");
        RunConvTest("5", "5", "5");
        RunConvTest(5.2, "5.2", "5.2");
        RunConvTest(5.299, "5.299", "5.299");
        RunConvTest(-5.2, "-5.2", "-5.2");

        // Now change threading culture
        Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture("de-DE");
        RunConvTest(5.2, "5.2", "5.2");
        RunConvTest(5.299, "5.299", "5.299");
        RunConvTest(-5.2, "-5.2", "-5.2");
    }

}