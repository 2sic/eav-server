namespace ToSic.Eav.DataSourceTests.TestData;

internal class RandomData
{
    internal static readonly Random Gen = new Random();

    internal static DateTime RandomDate()
    {
        var start = new DateTime(1995, 1, 1);
        var range = (DateTime.Today - start).Days;
        return start.AddDays(Gen.Next(range));
    }
}