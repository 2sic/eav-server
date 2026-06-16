

namespace ToSic.Sys.Utils.ObjectExtension;


public class ConvertToNumber: ConvertTestBase
{


    [Fact]
    public void StringToInt()
    {
        RunConvTest(null, 0, 0);
        RunConvTest("", 0, 0);
        RunConvTest("5", 5, 5);
        RunConvTest("5.2", 0, 5);
        RunConvTest("5.4", 0, 5);
        RunConvTest("5.5", 0, 6);
        RunConvTest("5.9", 0, 6);
        RunConvTest("   5.9", 0, 6);
        RunConvTest("5.9  ", 0, 6);
        RunConvTest("   5.9  ", 0, 6);
    }

    [Fact]
    public void StringToIntNull()
    {
        RunConvTest<int?>(null, null, null);
        RunConvTest<int?>("", null, null);
        RunConvTest<int?>("5", 5, 5);
        RunConvTest<int?>("5.2", null, 5);
        RunConvTest<int?>("5.4", null, 5);
        RunConvTest<int?>("5.5", null, 6);
        RunConvTest<int?>("5.9", null, 6);
    }

    [Fact]
    public void StringToFloat()
    {
        RunConvTest(null, 0f, 0f);
        RunConvTest("", 0f, 0f);
        RunConvTest("5", 5f, 5f);
        RunConvTest("5.2", 5.2f, 5.2f);
        RunConvTest("5.9", 5.9f, 5.9f);
        RunConvTest("-1", -1f, -1f);
        RunConvTest("-99.7", -99.7f, -99.7f);
    }

    [Fact]
    public void StringToDecimal()
    {
        RunConvTest(null, 0m, 0m);
        RunConvTest("", 0m, 0m);
        RunConvTest("5", 5m, 5m);
        RunConvTest("5.2", 5.2m, 5.2m);
        RunConvTest("5.9", 5.9m, 5.9m);
        RunConvTest("-1", -1m, -1m);
        RunConvTest("-99.7", -99.7m, -99.7m);
    }

    [Fact]
    public void StringToFloatNull()
    {
        RunConvTest<float?>(null, null, null);
        RunConvTest<float?>("", null, null);
        RunConvTest<float?>("5", 5f, 5f);
        RunConvTest<float?>("5.2", 5.2f, 5.2f);
        RunConvTest<float?>("5.9", 5.9f, 5.9f);
        RunConvTest<float?>("-1", -1f, -1f);
        RunConvTest<float?>("-99.7", -99.7f, -99.7f);
    }

    [Fact]
    public void StringToDecimalNull()
    {
        RunConvTest<decimal?>(null, null, null);
        RunConvTest<decimal?>("", null, null);
        RunConvTest<decimal?>("5", 5m, 5m);
        RunConvTest<decimal?>("5.2", 5.2m, 5.2m);
        RunConvTest<decimal?>("5.9", 5.9m, 5.9m);
        RunConvTest<decimal?>("-1", -1m, -1m);
        RunConvTest<decimal?>("-99.7", -99.7m, -99.7m);
    }
}