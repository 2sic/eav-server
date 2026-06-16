


namespace ToSic.Sys.Utils.ObjectExtension;


public class ConvertToBool: ConvertTestBase
{

    [Fact]
    public void NumberToBool()
    {
        RunConvTest(null, false, false, false);
        RunConvTest(0, false, false, false);
        RunConvTest(0.1, true, true, true);
        RunConvTest(-0.1, true, true, true);
        RunConvTest(1, true, true, true);
        RunConvTest(-1, true, true, true);
        RunConvTest(2, true, true, true);
        RunConvTest(2.5, true, true, true);
        RunConvTest(-3.7, true, true, true);
    }

    [Fact]
    public void StringToBool()
    {
        RunConvTest(null, false, false, false);
        RunConvTest("", false, false, false);
        RunConvTest("0", false, false, false);
        RunConvTest("1", false, false, true);
        RunConvTest("-1", false, false, true);
        RunConvTest("5", false, false, true);
        RunConvTest("5.2", false, false, true);
        RunConvTest("true", true, true, true);
        RunConvTest("True", true, true, true);
        RunConvTest("TRUE", true, true, true);
        RunConvTest("false", false, false, false);
        RunConvTest("False", false, false, false);
        RunConvTest("FALSE", false, false, false);
    }

    [Fact]
    public void StringToBoolNull()
    {
        True(0.ConvertOrDefaultTac<bool?>().HasValue);
        True("true".ConvertOrDefaultTac<bool?>().HasValue);

        RunConvTest<bool?>(null, null, null, null);
        RunConvTest<bool?>("", null, null, null);
        RunConvTest<bool?>("0", null, null, false);
        RunConvTest<bool?>("1", null, null, true);
        RunConvTest<bool?>("-1", null, null, true);
        RunConvTest<bool?>("5", null, null, true);
        RunConvTest<bool?>("5.2", null, null, true);
        RunConvTest<bool?>("true", true, true, true);
        RunConvTest<bool?>("True", true, true, true);
        RunConvTest<bool?>("TRUE", true, true, true);
        RunConvTest<bool?>("false", false, false, false);
        RunConvTest<bool?>("False", false, false, false);
        RunConvTest<bool?>("FALSE", false, false, false);
    }
}