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
    public void ParentScope_NewlyRegisteredTransient_IsNull()
        => Null(ParentService<MockChildScopeOnlyTransientBasic>());

    [Fact]
    public void ParentScope_ReRegisteredTransient_IsPreviousType()
        => IsType<MockTransientStandalone>(ParentService<IMockTransientStandalone>());

    [Fact]
    public void ParentScope_NewlyRegisteredScoped_IsNull()
        => Null(ParentService<MockChildScopeOnlyScopedBasic>());

    #endregion


    #region Verify Child Previous Registrations still work

    [Fact]
    public void ChildScope_PreviouslyRegisteredTransient_NotNull()
        => NotNull(ChildService<MockTransientStandalone>());

    [Fact]
    public void ChildScope_PreviouslyRegisteredScoped_NotNull()
        => NotNull(ChildService<MockScopedStandalone>());

    #endregion

    #region Verify Child Additions work

    [Fact]
    public void ChildScope_NewlyRegisteredTransient_NotNull()
        => NotNull(ChildService<MockChildScopeOnlyTransientBasic>());
    
    [Fact]
    public void ChildScope_NewlyRegisteredScoped_NotNull()
        => NotNull(ChildService<MockChildScopeOnlyScopedBasic>());

    [Fact]
    public void ChildScope_ReRegisteredTransient_NotNull()
        => NotNull(ChildService<IMockTransientStandalone>());

    [Fact]
    public void ChildScope_ReRegisteredTransient_IsNewType()
        => IsType<MockChildScopeOnlyTransientBasic>(ChildService<IMockTransientStandalone>());

    #endregion

    #region Verify Re-Registrations work as expected


    [Fact]
    public void ChildScope_ReRegisteredScoped_IsNewType()
    {
        // Arrange
        var services = BuildSps();
        var fromParent1 = services.Parent.GetService<MockScopedToReRegisterReqITransient>()!;
        var fromParent2 = services.Parent.GetService<MockScopedToReRegisterReqITransient>()!;
        
        // Pre-Verify that parent services are shared
        fromParent1.Value = 27;
        Equal(27, fromParent2.Value);
        
        var fromChild1 = services.Child.GetService<MockScopedToReRegisterReqITransient>()!;
        var fromChild2 = services.Child.GetService<MockScopedToReRegisterReqITransient>()!;
        fromChild1.Value = 42;
        Equal(42, fromChild2.Value);
        
        NotEqual(fromParent1.Value, fromChild2.Value);
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
        var transientProp = ChildService<MockScopedToReRegisterReqITransient>()!
            .TransientStandalone;
        
        NotEqual(ExpectedOutside, transientProp.Value);
        Equal(ExpectedInside, transientProp.Value);
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

}