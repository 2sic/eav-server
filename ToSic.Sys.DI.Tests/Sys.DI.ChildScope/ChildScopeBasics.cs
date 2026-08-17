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
        => Null(ParentService<MockChildTransientBasic>());

    [Fact]
    public void ParentScope_ReRegisteredTransient_IsPreviousType()
        => IsType<MockTransientStandalone>(ParentService<IMockTransientStandalone>());

    [Fact]
    public void ParentScope_NewlyRegisteredScoped_IsNull()
        => Null(ParentService<MockChildScopedBasic>());

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
        => NotNull(ChildService<MockChildTransientBasic>());
    
    [Fact]
    public void ChildScope_NewlyRegisteredScoped_NotNull()
        => NotNull(ChildService<MockChildScopedBasic>());

    [Fact]
    public void ChildScope_ReRegisteredTransient_NotNull()
        => NotNull(ChildService<IMockTransientStandalone>());

    [Fact]
    public void ChildScope_ReRegisteredTransient_IsNewType()
        => IsType<MockChildTransientBasic>(ChildService<IMockTransientStandalone>());

    #endregion

    #region Verify Re-Registrations work as expected


    [Fact]
    public void ChildScope_ReRegisteredScoped_IsNewType()
    {
        // Arrange
        var services = BuildSps();
        var fromParent1 = services.Parent.GetService<MockScopedStandaloneToReRegister>()!;
        var fromParent2 = services.Parent.GetService<MockScopedStandaloneToReRegister>()!;
        
        // Pre-Verify that parent services are shared
        fromParent1.Value = 27;
        Equal(27, fromParent2.Value);
        
        var fromChild1 = services.Child.GetService<MockScopedStandaloneToReRegister>()!;
        var fromChild2 = services.Child.GetService<MockScopedStandaloneToReRegister>()!;
        fromChild1.Value = 42;
        Equal(42, fromChild2.Value);
        
        NotEqual(fromParent1.Value, fromChild2.Value);
    }

    #endregion

    
}