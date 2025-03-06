using ToSic.Lib.DI.SwitchableServices.Services;
using static Xunit.Assert;

namespace ToSic.Lib.DI.SwitchableServices;

/// <summary>
/// Important: the methods are called in A-Z order, so they must preserve the names with the number to achieve this
/// </summary>
public class VerifySwitchableServiceSingleton(
    ServiceSwitcherSingleton<ITestSwitchableService> switcher1,
    ServiceSwitcherSingleton<ITestSwitchableService> switcher2,
    ServiceSwitcherSingleton<ITestSwitchableService> switcher3,
    ServiceSwitcherSingleton<ITestSwitchableService> switcher4
)
{
    /// <summary>
    /// This must run before everything
    /// </summary>
    [Fact]
    public void AccessSingletonN001() =>
        False(switcher1.IsValueCreated, "shouldn't be created at first");

    [Fact]
    public void AccessSingletonN002()
    {
        False(switcher2.IsValueCreated, "shouldn't be created at first");
        var x = switcher2.Value;
        True(switcher2.IsValueCreated, "should be created afterwards");
    }

    [Fact]
    public void AccessSingletonN003() =>
        True(switcher3.IsValueCreated, "should be created by now");

    [Fact]
    public void FindFallbackByName() =>
        Equal(TestSwitchableFallback.Name, switcher4.ByNameId(TestSwitchableFallback.Name).NameId);
}