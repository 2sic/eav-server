using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ToSic.Sys.DI.Override;

/// <summary>
/// Verify standard DI works, before trying to create sub-scopes.
/// </summary>
public class OverrideSubDependency(IServiceProvider sp)
{
    #region Test Classes

    private interface IMeal
    {
        int Value { get; set; }
    }

    private class MealDefault : IMeal
    {
        public const int InitialValue = 99262;
        public virtual int Value { get; set; } = InitialValue;
    }

    /// <summary>
    /// This will only be instantiated in the child scope through the interface, so it won't be discovered in the parent scope.
    /// </summary>
    private class MealBurger : MealDefault
    {
        public new const int InitialValue = 2603;
        public override int Value { get; set; } = InitialValue;
    }


    private class FoodBox(IMeal meal)
    {
        private const int InitialValue = 2963;
        public int Value { get; set; } = InitialValue;

        public IMeal Meal { get; } = meal;
    }
    
    #endregion

    public class Startup() : QuickStartup(services =>
    {
        services.TryAddTransient<MealDefault>();
        services.TryAddTransient(OverrideService<IMeal>.Register<MealDefault>());
        
        services.TryAddTransient<FoodBox>();
        services.TryAddTransient<MealBurger>();
    });

    #region Verify Re-Registrations work as expected

    private const int DefaultRegistrationValue = MealDefault.InitialValue;
    private const int ReplacedTypeValue = MealBurger.InitialValue;
    
    private IMeal GetTransientDependency()
        => sp.GetRequiredService<FoodBox>()!
            .Meal;

    

    private void Validate_MockTransient_WasReplaceWithOverride()
    {
        var transientProp = sp.GetRequiredService<FoodBox>()!
            .Meal;

        NotEqual(DefaultRegistrationValue, transientProp.Value);
        Equal(ReplacedTypeValue, transientProp.Value);
    }

    #region SwapWithDifferentMethods

    [Fact]
    public void Swap_WithType_GetsNewDependency()
    {
        using (OverrideService<IMeal>.Use<MealBurger>())
            Validate_MockTransient_WasReplaceWithOverride();
    }

    [Fact]
    public void Swap_WithFactory_GetsNewDependency()
    {
        using (OverrideService<IMeal>.Use(_ => new MealBurger()))
            Validate_MockTransient_WasReplaceWithOverride();
    }

    [Fact]
    public void Swap_WithValue_GetsNewDependency()
    {
        using (OverrideService<IMeal>.Use(new MealBurger()))
            Validate_MockTransient_WasReplaceWithOverride();
    }

    #endregion


    [Fact]
    public void Swap_BeforeAndAfterRemainUnchanged()
    {
        // Verify before
        Equal(DefaultRegistrationValue, GetTransientDependency().Value);

        // Override
        using (OverrideService<IMeal>.Use(_ => new MealBurger()))
            Validate_MockTransient_WasReplaceWithOverride();

        // Verify after
        Equal(DefaultRegistrationValue, GetTransientDependency().Value);
    }

    
    

    
    [Fact]
    public void Swap_WithFactory_RemainsTransient()
    {
        // Arrange
        using (OverrideService<IMeal>.Use(_ => new MealBurger()))
        {
            var transientProp = GetTransientDependency();
            transientProp.Value = 17;
            Equal(17, transientProp.Value);

            transientProp = GetTransientDependency();
            Equal(ReplacedTypeValue, transientProp.Value);
        }
    }

    [Fact]
    public void Swap_WithType_RemainsTransient()
    {
        // Arrange
        using (OverrideService<IMeal>.Use<MealBurger>())
        {
            var transientProp = GetTransientDependency();
            transientProp.Value = 17;
            Equal(17, transientProp.Value);

            transientProp = GetTransientDependency();
            Equal(MealBurger.InitialValue, transientProp.Value);
        }
    }


    [Fact]
    public void Swap_WithValue_BecomesScoped()
    {
        // Arrange
        var initial = 234;
        var modified = 19;
        using (OverrideService<IMeal>.Use(new MealBurger { Value = initial }))
        {
            var transientProp = GetTransientDependency();

            NotEqual(DefaultRegistrationValue, transientProp.Value);
            Equal(initial, transientProp.Value);
            
            // Now change value, and verify that the next instance still gets it
            transientProp.Value = modified;
            transientProp = GetTransientDependency();
            Equal(modified, transientProp.Value);
        }
    }
    


    #endregion

}