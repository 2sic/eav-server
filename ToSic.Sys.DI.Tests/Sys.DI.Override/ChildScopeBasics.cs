using Microsoft.Extensions.DependencyInjection;
using ToSic.Mock.LifetimeServices;
using static ToSic.Sys.DI.Override.ChildScopeSetup;

namespace ToSic.Sys.DI.Override;

/// <summary>
/// Verify standard DI works, before trying to create sub-scopes.
/// </summary>
public class ChildScopeBasics
{
    #region Verify Re-Registrations work as expected
    
    private IMockTransientStandalone GetTransientDependency(IServiceProvider sp)
        => sp.GetService<MockTransientRequiringTransient>()!
            .TransientStandalone;

    private const int DefaultRegistrationValue = MockTransientStandalone.InitialValue;
    private const int ReplacedTypeValue = MockChildScopeOnlyTransientBasic.InitialValue;

    [Fact]
    public void ReRegisteredScoped_GetsNewDependency()
    {
        // Arrange
        // TODO:
        using (OverrideService<IMockTransientStandalone>.Use<MockChildScopeOnlyTransientBasic>())
        {
            var transientProp = GetService<IMockScopedToReRegisterReqITransient>()!
                .TransientStandalone;

            NotEqual(DefaultRegistrationValue, transientProp.Value);
            Equal(ReplacedTypeValue, transientProp.Value);
        }
    }
    
    [Fact]
    public void PreviouslyRegisteredTransient_BeforeAndAfter()
    {
        // Arrange
        var sp = BuildSps();
        
        // Verify before
        Equal(DefaultRegistrationValue, GetTransientDependency(sp).Value);
        
        using (OverrideService<IMockTransientStandalone>.Use(_ => new MockChildScopeOnlyTransientBasic()))
        {
            var transientProp = GetTransientDependency(sp);

            NotEqual(DefaultRegistrationValue, transientProp.Value);
            Equal(ReplacedTypeValue, transientProp.Value);
        }

        // Verify after
        Equal(DefaultRegistrationValue, GetTransientDependency(sp).Value);
    }

    [Fact]
    public void PreviouslyRegisteredTransient_Factory_GetsNewDependency()
    {
        // Arrange
        using (OverrideService<IMockTransientStandalone>.Use(_ => new MockChildScopeOnlyTransientBasic()))
        {
            var transientProp = GetTransientDependency(BuildSps());

            NotEqual(DefaultRegistrationValue, transientProp.Value);
            Equal(ReplacedTypeValue, transientProp.Value);
        }
    }

    [Fact]
    public void PreviouslyRegisteredTransient_Factory_RemainsTransient()
    {
        // Arrange
        var services = BuildSps();
        using (OverrideService<IMockTransientStandalone>.Use(_ => new MockChildScopeOnlyTransientBasic()))
        {
            var transientProp = GetTransientDependency(services);
            transientProp.Value = 17;
            Equal(17, transientProp.Value);

            transientProp = GetTransientDependency(services);
            Equal(ReplacedTypeValue, transientProp.Value);
        }
    }
    
    [Fact]
    public void PreviouslyRegisteredTransient_WithValue_GetsNewDependency()
    {
        // Arrange
        var services = BuildSps();
        using (OverrideService<IMockTransientStandalone>.Use(new MockChildScopeOnlyTransientBasic()))
        {
            var transientProp = GetTransientDependency(services);

            NotEqual(DefaultRegistrationValue, transientProp.Value);
            Equal(ReplacedTypeValue, transientProp.Value);
        }
    }
    
    [Fact]
    public void PreviouslyRegisteredTransient_WithValue_BecomesScoped()
    {
        // Arrange
        var services = BuildSps();
        var initial = 234;
        var modified = 19;
        using (OverrideService<IMockTransientStandalone>.Use(new MockChildScopeOnlyTransientBasic { Value = initial }))
        {
            var transientProp = GetTransientDependency(services);

            NotEqual(DefaultRegistrationValue, transientProp.Value);
            Equal(initial, transientProp.Value);
            
            // Now change value, and verify that the next instance still gets it
            transientProp.Value = modified;
            transientProp = GetTransientDependency(services);
            Equal(modified, transientProp.Value);
        }
    }
    
    [Fact]
    public void PreviouslyRegisteredTransient_Type_GetsNewDependency()
    {
        // Arrange
        var services = BuildSps();
        using (OverrideService<IMockTransientStandalone>.Use<MockChildScopeOnlyTransientPreRegistered>())
        {
            var transientProp = GetTransientDependency(services);

            NotEqual(DefaultRegistrationValue, transientProp.Value);
            Equal(MockChildScopeOnlyTransientPreRegistered.InitialValue, transientProp.Value);
        }
    }

    [Fact]
    public void PreviouslyRegisteredTransient_Type_RemainsTransient()
    {
        // Arrange
        var services = BuildSps();
        using (OverrideService<IMockTransientStandalone>.Use<MockChildScopeOnlyTransientPreRegistered>())
        {
            var transientProp = GetTransientDependency(services);
            transientProp.Value = 17;
            Equal(17, transientProp.Value);

            transientProp = GetTransientDependency(services);
            Equal(MockChildScopeOnlyTransientPreRegistered.InitialValue, transientProp.Value);
        }
    }

    #endregion


    // TODO:
    // - previously registered services asking for a new dependency, should get the new dependency
    // - anything asking for the service provider should get the latest
    // - stacked overrides! ⚠️

}