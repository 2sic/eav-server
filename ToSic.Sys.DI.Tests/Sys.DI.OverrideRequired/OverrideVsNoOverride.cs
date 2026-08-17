using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ToSic.Sys.DI.OverrideRequired;

public class OverrideVsNoOverride(IServiceProvider sp)
{
    #region Classes / Interfaces

    private interface IInterface
    {
        int Value { get; set; }
    }

    private class Implementation() : IInterface
    {
        private const int InitialValue = 602305;
        public int Value { get; set; } = InitialValue;
    }

    #endregion

    #region Setup

    public class Startup() : QuickStartup(services =>
    {
        // Note: any scoped which we would ever want to override
        // must be registered as transient, and just created once.
        // so we could have the implementation as scoped
        //services.TryAddScoped(OverrideService<IMockScopedToReRegisterReqITransient>.Register<MockScopedToReRegisterReqITransient>());
        services.TryAddTransient<Implementation>();
        services.TryAddTransient(OverrideService<IInterface>.RegisterScoped<Implementation>());
    });

    #endregion


    [Fact]
    public void Parents_Share()
    {
        // Check before override, that the parent services are shared
        var fromParent1 = sp.GetService<IInterface>()!;
        var fromParent2 = sp.GetService<IInterface>()!;

        // Pre-Verify that parent services are shared
        fromParent1.Value = 27;
        Equal(27, fromParent1.Value);
        Equal(27, fromParent2.Value);
    }
    
    [Fact]
    public void Override_Share()
    {
        using (OverrideService<IInterface>.Use(new Implementation()))
        {
            var fromChild1 = sp.GetService<IInterface>()!;
            var fromChild2 = sp.GetService<IInterface>()!;
            fromChild1.Value = 42;
            Equal(42, fromChild2.Value);
        }

    }
    
    [Fact]
    public void Parent_Override_DontShare()
    {
        // Check before override, that the parent services are shared
        var fromParent1 = sp.GetService<IInterface>()!;
        fromParent1.Value = 27;
        Equal(27, fromParent1.Value);

        using (OverrideService<IInterface>.Use(new Implementation()))
        {
            var fromChild1 = sp.GetService<IInterface>()!;
            fromChild1.Value = 42;
            
            // The child must have isolated objects
            NotSame(fromChild1, fromParent1);
            NotEqual(fromParent1.Value, fromChild1.Value);
        }

    }
    
}