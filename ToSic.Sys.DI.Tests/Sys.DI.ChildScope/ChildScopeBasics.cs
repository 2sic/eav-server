using Microsoft.Extensions.DependencyInjection;
using ToSic.Mock.LifetimeServices;
using static ToSic.Sys.DI.ChildScope.ChildScopeSetup;

namespace ToSic.Sys.DI.ChildScope;

/// <summary>
/// Verify standard DI works, before trying to create sub-scopes.
/// </summary>
public class ChildScopeBasics
{
    #region Verify Parent is unchanged

    [Fact]
    public void ParentScope_ReRegisteredTransient_IsPreviousType()
        => IsType<MockTransientStandalone>(ParentService<IMockTransientStandalone>());

    [Fact]
    public void ParentScope_NewlyRegisteredScoped_Throws()
        => Throws<NotSupportedException>(ParentService<IMockChildScopeOnlyScopedBasic>);

    #endregion


    #region Verify Child Additions work

    [Fact]
    public void ChildScope_NewlyRegisteredScoped_NotNull()
    {
        using (OverrideContext<IMockChildScopeOnlyScopedBasic>.Begin<MockChildScopeOnlyScopedBasic>())
        {
            NotNull(ParentService<IMockChildScopeOnlyScopedBasic>());
        }
    }


    [Fact]
    public void ChildScope_ReRegisteredTransient_IsNewType()
    {
        using (OverrideContext<IMockTransientStandalone>.Begin(_ => new MockChildScopeOnlyTransientBasic()))
        {
            IsType<MockChildScopeOnlyTransientBasic>(ParentService<IMockTransientStandalone>());
        }
    }

    #endregion

    #region Verify Re-Registrations work as expected


    [Fact]
    public void ChildScope_ReRegisteredScoped_IsNewType()
    {
        // Arrange
        var services = BuildSps();
        var fromParent1 = services.Parent.GetService<IMockScopedToReRegisterReqITransient>()!;
        var fromParent2 = services.Parent.GetService<IMockScopedToReRegisterReqITransient>()!;
        
        // Pre-Verify that parent services are shared
        fromParent1.Value = 27;
        Equal(27, fromParent1.Value);
        Equal(27, fromParent2.Value);

        using (OverrideContext<IMockScopedToReRegisterReqITransient>.Begin(new MockScopedToReRegisterReqITransient(null!)))
        {
            var fromChild1 = services.Parent.GetService<IMockScopedToReRegisterReqITransient>()!;
            var fromChild2 = services.Parent.GetService<IMockScopedToReRegisterReqITransient>()!;
            fromChild1.Value = 42;
            Equal(42, fromChild2.Value);
            
            // The child must have isolated objects
            NotSame(fromChild1, fromParent1);
            NotEqual(fromParent1.Value, fromChild2.Value);
        }

    }
    
    private IMockTransientStandalone GetTransChild(IServiceProvider sp)
        => sp.GetService<MockTransientRequiringTransient>()!
            .TransientStandalone;

    private const int ExpectedOutside = MockTransientStandalone.InitialValue;
    private const int ExpectedInside = MockChildScopeOnlyTransientBasic.InitialValue;

    [Fact]
    public void ChildScope_ReRegisteredScoped_GetsNewDependency()
    {
        // Arrange
        // TODO:
        using (OverrideContext<IMockTransientStandalone>.Begin<MockChildScopeOnlyTransientBasic>())
        {
            var transientProp = ParentService<IMockScopedToReRegisterReqITransient>()!
                .TransientStandalone;

            NotEqual(ExpectedOutside, transientProp.Value);
            Equal(ExpectedInside, transientProp.Value);
        }
    }
    
    [Fact]
    public void ChildScope_PreviouslyRegisteredTransient_BeforeAndAfter()
    {
        // Arrange
        var sp = BuildSps().Parent;
        
        // Verify before
        Equal(ExpectedOutside, GetTransChild(sp).Value);
        
        using (OverrideContext<IMockTransientStandalone>.Begin(_ => new MockChildScopeOnlyTransientBasic()))
        {
            var transientProp = GetTransChild(sp);

            NotEqual(ExpectedOutside, transientProp.Value);
            Equal(ExpectedInside, transientProp.Value);
        }

        // Verify after
        Equal(ExpectedOutside, GetTransChild(sp).Value);
    }

    [Fact]
    public void ChildScope_PreviouslyRegisteredTransient_Factory_GetsNewDependency()
    {
        // Arrange
        using (OverrideContext<IMockTransientStandalone>.Begin(_ => new MockChildScopeOnlyTransientBasic()))
        {
            var transientProp = GetTransChild(BuildSps().Parent);

            NotEqual(ExpectedOutside, transientProp.Value);
            Equal(ExpectedInside, transientProp.Value);
        }
    }

    [Fact]
    public void ChildScope_PreviouslyRegisteredTransient_Factory_RemainsTransient()
    {
        // Arrange
        var services = BuildSps();
        using (OverrideContext<IMockTransientStandalone>.Begin(_ => new MockChildScopeOnlyTransientBasic()))
        {
            var transientProp = GetTransChild(services.Parent);
            transientProp.Value = 17;
            Equal(17, transientProp.Value);

            transientProp = GetTransChild(services.Parent);
            Equal(ExpectedInside, transientProp.Value);
        }
    }
    
    [Fact]
    public void ChildScope_PreviouslyRegisteredTransient_Value_GetsNewDependency()
    {
        // Arrange
        var services = BuildSps();
        using (OverrideContext<IMockTransientStandalone>.Begin(new MockChildScopeOnlyTransientBasic()))
        {
            var transientProp = GetTransChild(services.Parent);

            NotEqual(ExpectedOutside, transientProp.Value);
            Equal(ExpectedInside, transientProp.Value);
        }
    }
    
    [Fact]
    public void ChildScope_PreviouslyRegisteredTransient_Value_BecomesScoped()
    {
        // Arrange
        var services = BuildSps();
        var initial = 234;
        var modified = 19;
        using (OverrideContext<IMockTransientStandalone>.Begin(new MockChildScopeOnlyTransientBasic { Value = initial }))
        {
            var transientProp = GetTransChild(services.Parent);

            NotEqual(ExpectedOutside, transientProp.Value);
            Equal(initial, transientProp.Value);
            
            // Now change value, and verify that the next instance still gets it
            transientProp.Value = modified;
            transientProp = GetTransChild(services.Parent);
            Equal(modified, transientProp.Value);
        }
    }
    
    [Fact]
    public void ChildScope_PreviouslyRegisteredTransient_Type_GetsNewDependency()
    {
        // Arrange
        var services = BuildSps();
        using (OverrideContext<IMockTransientStandalone>.Begin<MockChildScopeOnlyTransientPreRegistered>())
        {
            var transientProp = GetTransChild(services.Parent);

            NotEqual(ExpectedOutside, transientProp.Value);
            Equal(MockChildScopeOnlyTransientPreRegistered.InitialValue, transientProp.Value);
        }
    }

    [Fact]
    public void ChildScope_PreviouslyRegisteredTransient_Type_RemainsTransient()
    {
        // Arrange
        var services = BuildSps();
        using (OverrideContext<IMockTransientStandalone>.Begin<MockChildScopeOnlyTransientPreRegistered>())
        {
            var transientProp = GetTransChild(services.Parent);
            transientProp.Value = 17;
            Equal(17, transientProp.Value);

            transientProp = GetTransChild(services.Parent);
            Equal(MockChildScopeOnlyTransientPreRegistered.InitialValue, transientProp.Value);
        }
    }

    #endregion


    // TODO:
    // - previously registered services asking for a new dependency, should get the new dependency
    // - anything asking for the service provider should get the latest
    // - stacked overrides! ⚠️

}