using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ToSic.Mock.LifetimeServices;

namespace ToSic.Sys.DI.Override;

/// <summary>
/// Verify standard DI works, before trying to create sub-scopes.
/// </summary>
public class StandardDiBasics(IServiceProvider sp)
{
    public class Startup() : QuickStartup(services =>
    {
        services.TryAddTransient<MockTransientStandalone>();
        services.TryAddScoped<MockScopedStandalone>();
    });

    [Fact]
    public void Standard_Verify_Transient_NotNull()
        => NotNull(sp.GetService<MockTransientStandalone>());

    [Fact]
    public void Standard_Verify_Transient_NotShared()
    {
        var first = sp.GetRequiredService<MockTransientStandalone>();
        Equal(MockTransientStandalone.InitialValue, first.Value);
        first.Value = 27;
        var second = sp.GetRequiredService<MockTransientStandalone>();
        Equal(MockTransientStandalone.InitialValue, second.Value);
    }

    [Fact]
    public void Standard_Verify_Scoped_NotNull()
        => NotNull(sp.GetService<MockScopedStandalone>());

    [Fact]
    public void Standard_Verify_Scoped_IsShared()
    {
        var first = sp.GetRequiredService<MockScopedStandalone>();
        Equal(MockScopedStandalone.InitialValue, first.Value);
        first.Value = 27;
        var second = sp.GetRequiredService<MockScopedStandalone>();
        Equal(27, second.Value);
    }
}