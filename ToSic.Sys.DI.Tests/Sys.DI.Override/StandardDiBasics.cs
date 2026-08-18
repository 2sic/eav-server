using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ToSic.Sys.DI.Override;

/// <summary>
/// Verify standard DI works, before trying to create sub-scopes.
/// </summary>
public class StandardDiBasics(IServiceProvider sp)
{
    #region Test Classes

    private class MockCarTransient
    {
        public const int InitialValue = 99262;
        public int Value { get; set; } = InitialValue;
    }

    private class MockRoadScoped
    {
        public const int InitialValue = 59302;
        public int Value { get; set; } = InitialValue;
    }
    
    #endregion

    
    #region Startup

    public class Startup() : QuickStartup(services =>
    {
        services.TryAddTransient<MockCarTransient>();
        services.TryAddScoped<MockRoadScoped>();
    });

    #endregion


    [Fact]
    public void Standard_Verify_Transient_NotNull()
        => NotNull(sp.GetService<MockCarTransient>());

    [Fact]
    public void Standard_Verify_Transient_NotShared()
    {
        var first = sp.GetRequiredService<MockCarTransient>();
        Equal(MockCarTransient.InitialValue, first.Value);
        first.Value = 27;
        var second = sp.GetRequiredService<MockCarTransient>();
        Equal(MockCarTransient.InitialValue, second.Value);
    }

    [Fact]
    public void Standard_Verify_Scoped_NotNull()
        => NotNull(sp.GetService<MockRoadScoped>());

    [Fact]
    public void Standard_Verify_Scoped_IsShared()
    {
        var first = sp.GetRequiredService<MockRoadScoped>();
        Equal(MockRoadScoped.InitialValue, first.Value);
        first.Value = 27;
        var second = sp.GetRequiredService<MockRoadScoped>();
        Equal(27, second.Value);
    }
}