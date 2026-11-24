using Microsoft.Extensions.DependencyInjection.Extensions;
using ToSic.Sys.Run.Startup;

namespace ToSic.Sys.DI.Override;

public class CtxOverrideTests(ISysServiceProvider sp)
{
    #region Test Classes

    internal abstract class Animal
    {
        public string Name => GetType().Name;
    }

    private class AnimalDefault : Animal;

    private class AnimalDog : Animal;

    private class AnimalMonkey : Animal;

    internal class Cage(Animal animal)
    {
        public Animal Animal { get; } = animal;
    }

    #endregion

    public class Startup() : QuickStartup(services =>
    {
        services.AddSysCoreDi();

        // Register an empty Ctx so that asking for it in DI doesn't throw an error
        // It will still not work unless the specific Ctx is registered,
        // but it will allow for clearer error messages to indicate missing registration.
        services.TryAddTransient(typeof(Ctx<>));
        // services.TryAddTransient(OverrideService < Ctx<,> > typeof(Ctx<>));
        services.TryAddTransient(Ctx<Animal>.Register());
        services.TryAddTransient<Cage>();

        services.TryAddTransient<AnimalDefault>();
        services.TryAddTransient(OverrideService<Animal>.Register<AnimalDefault>());
        services.TryAddTransient<AnimalDog>();
        services.TryAddTransient<AnimalMonkey>();
    });

    
    [Fact]
    public void RegisteredForgotGivesProperException()
    {
        var x = sp.Build<Ctx<Cage>>();
        Throws<CtxNotRegisteredException>(() => x.Value);
    }

    [Fact]
    public void RegisteredCorrectForgotSetGivesProperException()
    {
        var x = sp.Build<Ctx<Animal>>();
        Throws<CtxRegisteredButNotAssignedException>(() => x.Value);
    }

    [Fact]
    public void Test2()
    {
        var dog = sp.Build<AnimalDog>();
        using (OverrideService<Ctx<Animal>>.Use(Ctx<Animal>.Transport(dog)))
        {
            var x = sp.Build<Ctx<Animal>>();
            Equal("AnimalDog", x.Value.Name);
        }
    }

}
