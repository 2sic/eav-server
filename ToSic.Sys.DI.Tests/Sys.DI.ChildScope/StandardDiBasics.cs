using Microsoft.Extensions.DependencyInjection;
using ToSic.Mock.LifetimeServices;

namespace ToSic.Sys.DI.ChildScope;

/// <summary>
/// Verify standard DI works, before trying to create sub-scopes.
/// </summary>
public class StandardDiBasics
{

    private static ServiceProvider BuildSp()
        => new ServiceCollection()
            .AddMockLifetimes()
            .BuildServiceProvider();

    [Fact]
    public void Standard_Verify_Transient_NotNull()
        => NotNull(BuildSp().GetService<MockTransientStandalone>());

    [Fact]
    public void Standard_Verify_Transient_NotShared()
    {
        var sp = BuildSp();
        var first = sp.GetRequiredService<MockTransientStandalone>();
        Equal(MockTransientStandalone.InitialValue, first.Value);
        first.Value = 27;
        var second = sp.GetRequiredService<MockTransientStandalone>();
        Equal(MockTransientStandalone.InitialValue, second.Value);
    }

    [Fact]
    public void Standard_Verify_Scoped_NotNull()
        => NotNull(BuildSp().GetService<MockScopedStandalone>());

    [Fact]
    public void Standard_Verify_Scoped_IsShared()
    {
        var sp = BuildSp();
        var first = sp.GetRequiredService<MockScopedStandalone>();
        Equal(MockScopedStandalone.InitialValue, first.Value);
        first.Value = 27;
        var second = sp.GetRequiredService<MockScopedStandalone>();
        Equal(27, second.Value);
    }
}