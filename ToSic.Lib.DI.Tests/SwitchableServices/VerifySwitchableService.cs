using ToSic.Lib.DI.SwitchableServices.Services;
using static Xunit.Assert;

namespace ToSic.Lib.DI.SwitchableServices;

public class VerifySwitchableService(ServiceSwitcher<ITestSwitchableService> switcher)
{

    [Fact]
    public void FindKeepService() =>
        Equal(TestSwitchableKeep.Name, switcher.Value.NameId);

    [Fact]
    public void Has3Services() =>
        Equal(3, switcher.AllServices.Count);

    [Fact]
    public void NotCreateBeforeButCreatedAfter()
    {
        False(switcher.IsValueCreated, "shouldn't be created at first");
        var x = switcher.Value;
        True(switcher.IsValueCreated, "should be created afterwards");
    }

    [Fact]
    public void FindFallbackByName() =>
        Equal(TestSwitchableFallback.Name, switcher.ByNameId(TestSwitchableFallback.Name).NameId);
}