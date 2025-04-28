using System.Globalization;

namespace ToSic.Eav.Plumbing.ObjectExtension;


public class ConvertToString: ConvertTestBase
{
    [Fact (Skip = "ATM not ready, won't do what we would like but not sure if this is even relevant")]
    public void DateTimeToString()
    {
        ConvT(new DateTime(2021,09,29), "2021-09-29", "2021-09-29");
    }

    [Fact]
    public void StringToString()
    {
        Assert.Equal(null, (null as string).ConvertOrDefaultTac<string>());
        Assert.Equal("", "".ConvertOrDefaultTac<string>());
        Assert.Equal("5", "5".ConvertOrDefaultTac<string>());
    }


    [Fact]
    public void NumberToString()
    {
        ConvT(null, null as string, null);
        ConvT("", "", "");
        ConvT("5", "5", "5");
        ConvT(5.2, "5.2", "5.2");
        ConvT(5.299, "5.299", "5.299");
        ConvT(-5.2, "-5.2", "-5.2");

        // Now change threading culture
        System.Threading.Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture("de-DE");
        ConvT(5.2, "5.2", "5.2");
        ConvT(5.299, "5.299", "5.299");
        ConvT(-5.2, "-5.2", "-5.2");
    }

}