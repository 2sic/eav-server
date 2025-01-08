using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Testing.Shared;
using static Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace ToSic.Eav.DataSourceTests.BaseClassTests;

[TestClass]
public class GetThisTests: TestBaseEavDataSource
{
    [TestMethod] public void GetThisString() => AreEqual(TestDsGetThis.ExpectedGetThisString, GetPropsDs().GetThisString);

    [TestMethod] public void GetThisTrue() => AreEqual(true, GetPropsDs().GetThisTrue);

    [TestMethod] public void GetThisFalseDefault() => AreEqual(false, GetPropsDs().GetThisFalseDefault);
    [TestMethod] public void GetThisFalseInitialized() => AreEqual(false, GetPropsDs().GetThisFalseInitialized);


    private TestDsGetThis GetPropsDs()
    {
        var ds = CreateDataSource<TestDsGetThis>();
        ds.Configuration.Parse();
        return ds;
    }
}