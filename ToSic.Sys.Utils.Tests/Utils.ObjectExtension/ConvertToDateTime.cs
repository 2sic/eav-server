namespace ToSic.Sys.Utils.ObjectExtension;


public class ConvertToDateTime: ConvertTestBase
{

    [Fact]
    public void NullToDateTime()
    {
        RunConvTest(null, DateTime.MinValue, DateTime.MinValue, DateTime.MinValue);
    }

    [Fact]
    public void StringDateToDateTime()
    {
        RunConvTest("", DateTime.MinValue, DateTime.MinValue, DateTime.MinValue);
        RunConvTest("2021-01-01", new(2021, 1, 1), new DateTime(2021, 1, 1));
        RunConvTest("2021-12-31", new(2021, 12, 31), new DateTime(2021, 12, 31));
    }

    [Fact]
    public void StringToDateTime()
    {
        RunConvTest("", DateTime.MinValue, DateTime.MinValue, DateTime.MinValue);
        RunConvTest("2021-01-01 10:42", new(2021, 1, 1, 10, 42, 00), new DateTime(2021, 1, 1, 10, 42, 00));
        RunConvTest("2021-01-01 10:42:00", new(2021, 1, 1, 10, 42, 00), new DateTime(2021, 1, 1, 10, 42, 00));
        RunConvTest("2021-01-01 10:42:27", new(2021, 1, 1, 10, 42, 27), new DateTime(2021, 1, 1, 10, 42, 27));
    }
      
}