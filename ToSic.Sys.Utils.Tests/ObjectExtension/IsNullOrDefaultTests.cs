namespace ToSic.Eav.Plumbing.ObjectExtension;


public class IsNullOrDefaultTests
{
    [Fact]
    public void IsNullOrDefaultStrings()
    {
        Test3(null, true);
        Test3("", false);
        Test3("test", false);
    }


    [Fact]
    public void IsNullOrDefaultInt()
    {
        Test3(0, true);
        Test3(5, false);
        Test3(27, false);
    }

    [Fact]
    public void IsNullOrDefaultBool()
    {
        Test3(null as bool?, true);
        Test3(true, false);
        Test3(true, false, expectedOnFaDFalse: false);
        Test3(false, false, expectedOnFaDFalse: true);
    }

    [Fact]
    public void IsNullOrDefaultListInt()
    {
        Test3(null as List<int>, true);
        Test3(new List<int>(), false);
        Test3(default(List<int>), true);
    }

    [Fact]
    public void IsNullOrDefaultKeyValuePair()
    {
        // null cannot be converted to KeyValuePair so this test can't run
        //TripleTest(null as KeyValuePair<string,string>, true);
        Test3(default(KeyValuePair<string,string>), true);
    }

    private void Test3(object value, bool expected, bool? expectedOnFaDTrue = null, bool? expectedOnFaDFalse = null)
    {
        Equal(expected, value.IsNullOrDefaultTac());
        Equal(expectedOnFaDTrue ?? expected, value.IsNullOrDefaultTac(true));
        Equal(expectedOnFaDFalse ?? expected, value.IsNullOrDefaultTac(false));
    }

}