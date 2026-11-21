using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;


namespace ToSic.Sys.DI.Override;

public class OverrideLazy(IServiceProvider sp)
{
    /// <summary>
    /// This will be the name of an animal if the override mechanism fails,
    /// since it will return a <see cref="AnimalDefault"/>.
    /// </summary>
    private const string NameIfLazyFails = nameof(AnimalDefault);
    #region Classes

    internal interface IAnimal
    {
        string Name { get; }
        IAnimal ClassicLazyChild { get; }
        IAnimal SuperLazyChild { get; }
    }

    private abstract class Animal(Lazy<IAnimal> lazy, LazySvc<IAnimal> lazier) : IAnimal
    {
        public string Name => GetType().Name;
        
        public IAnimal ClassicLazyChild => lazy.Value;
        public IAnimal SuperLazyChild => lazier.Value;
    }

    private class AnimalDefault(Lazy<IAnimal> lazy, LazySvc<IAnimal> lazier) : Animal(lazy, lazier);
    private class AnimalDog(Lazy<IAnimal> lazy, LazySvc<IAnimal> lazier) : Animal(lazy, lazier);
    private class AnimalMonkey(Lazy<IAnimal> lazy, LazySvc<IAnimal> lazier) : Animal(lazy, lazier);
    private class AnimalElephant(Lazy<IAnimal> lazy, LazySvc<IAnimal> lazier) : Animal(lazy, lazier);
    private class AnimalBird(Lazy<IAnimal> lazy, LazySvc<IAnimal> lazier) : Animal(lazy, lazier);

    internal class Cage(Lazy<IAnimal> animal, LazySvc<IAnimal> lazier)
    {
        public IAnimal ClassicLazyAnimal => animal.Value;
        public IAnimal SuperLazyAnimal => lazier.Value;
    }

    #endregion

    public class Startup() : QuickStartup(services =>
    {
        services.TryAddTransient(typeof(Lazy<>), typeof(LazyImplementation<>));
        services.TryAddTransient(typeof(LazySvc<>));

        // Register the default animal, and the default service for IAnimal
        services.TryAddTransient<AnimalDefault>();
        services.TryAddTransient(OverrideService<IAnimal>.Register<AnimalDefault>());

        // Register the remaining animals, so we can override them in the tests
        services.TryAddTransient<Cage>();
        services.TryAddTransient<AnimalDog>();
        services.TryAddTransient<AnimalMonkey>();
        services.TryAddTransient<AnimalElephant>();
        services.TryAddTransient<AnimalBird>();
    });


    

    [Fact]
    public void LazyInsideAllVariantsWork()
    {
        using (OverrideService<IAnimal>.Use<AnimalDog>())
        {
            var evalInside = sp.GetRequiredService<Cage>();
            Equal(nameof(AnimalDog), evalInside.ClassicLazyAnimal.Name);
            Equal(nameof(AnimalDog), evalInside.SuperLazyAnimal.Name);
            
            Equal(nameof(AnimalDog), evalInside.ClassicLazyAnimal.ClassicLazyChild.Name);
            Equal(nameof(AnimalDog), evalInside.ClassicLazyAnimal.SuperLazyChild.Name);
        }
    }

    [Fact]
    public void LazyOutsideClassicFails()
    {
        Cage? evalOutside;
        using (OverrideService<IAnimal>.Use<AnimalDog>())
        {
            evalOutside = sp.GetService<Cage>();
        }
        
        // Standard lazy fails, for child and grand child
        Equal(NameIfLazyFails, evalOutside?.ClassicLazyAnimal.Name);
        Equal(NameIfLazyFails, evalOutside?.ClassicLazyAnimal.ClassicLazyChild.Name);
        Equal(NameIfLazyFails, evalOutside?.ClassicLazyAnimal.SuperLazyChild.Name);
    }

    [Fact]
    public void LazyOutsideSuperWorks()
    {
        Cage? evalOutside;
        using (OverrideService<IAnimal>.Use<AnimalDog>())
        {
            evalOutside = sp.GetService<Cage>();
        }
        
        // Works
        Equal(nameof(AnimalDog), evalOutside?.SuperLazyAnimal.Name);
        Equal(nameof(AnimalDog), evalOutside?.SuperLazyAnimal.SuperLazyChild.Name);
        Equal(nameof(AnimalDog), evalOutside?.SuperLazyAnimal.SuperLazyChild.SuperLazyChild.Name);
        
    }

    [Fact]
    public void LazyOutsideWithInnerClassicFails()
    {
        Cage? evalOutside;
        using (OverrideService<IAnimal>.Use<AnimalDog>())
        {
            evalOutside = sp.GetService<Cage>();
        }
        
        // If lazy again deep down, that level will not work
        Equal(NameIfLazyFails, evalOutside?.SuperLazyAnimal.ClassicLazyChild.Name);
    }
}
