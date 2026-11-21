using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;


namespace ToSic.Sys.DI.Override;

public class OverrideLazy(IServiceProvider sp)
{
    #region Classes

    internal interface IAnimal
    {
        string Name { get; }
        IAnimal LazyChild { get; }
        IAnimal LazierChild { get; }
    }

    private abstract class Animal(Lazy<IAnimal> lazy, LazySupportingOverride<IAnimal> lazier) : IAnimal
    {
        public string Name => GetType().Name.Substring("Animal".Length);
        
        public IAnimal LazyChild => lazy.Value;
        public IAnimal LazierChild => lazier.Value;
    }

    private class AnimalDefault(Lazy<IAnimal> lazy, LazySupportingOverride<IAnimal> lazier) : Animal(lazy, lazier);
    private class AnimalDog(Lazy<IAnimal> lazy, LazySupportingOverride<IAnimal> lazier) : Animal(lazy, lazier);
    private class AnimalMonkey(Lazy<IAnimal> lazy, LazySupportingOverride<IAnimal> lazier) : Animal(lazy, lazier);
    private class AnimalElephant(Lazy<IAnimal> lazy, LazySupportingOverride<IAnimal> lazier) : Animal(lazy, lazier);
    private class AnimalBird(Lazy<IAnimal> lazy, LazySupportingOverride<IAnimal> lazier) : Animal(lazy, lazier);

    internal class Cage(Lazy<IAnimal> animal, LazySupportingOverride<IAnimal> lazier)
    {
        public IAnimal LazyAnimal => animal.Value;
        public IAnimal LazierAnimal => lazier.Value;
    }

    #endregion

    public class Startup() : QuickStartup(services =>
    {
        services.TryAddTransient(typeof(Lazy<>), typeof(LazyImplementation<>));
        services.TryAddTransient(typeof(LazySupportingOverride<>));


        services.TryAddTransient<AnimalDefault>();
        services.TryAddTransient(OverrideService<IAnimal>.Register<AnimalDefault>());

        services.TryAddTransient<Cage>();
        services.TryAddTransient<AnimalDog>();
        services.TryAddTransient<AnimalMonkey>();
        services.TryAddTransient<AnimalElephant>();
        services.TryAddTransient<AnimalBird>();
    });


    

    [Fact]
    public void LazyInsideWorks()
    {
        Cage? evalOutside;
        using (OverrideService<IAnimal>.Use<AnimalDog>())
        {
            var evalInside = sp.GetRequiredService<Cage>();
            Equal("Dog", evalInside.LazyAnimal.Name);
            Equal("Dog", evalInside.LazierAnimal.Name);
            
            Equal("Dog", evalInside.LazyAnimal.LazyChild.Name);
            Equal("Dog", evalInside.LazyAnimal.LazierChild.Name);
            
            evalOutside = sp.GetService<Cage>();
        }
        
        //// Standard lazy fails, for child and grand child
        //Equal("Default", evalOutside?.LazyAnimal.Name);
        //Equal("Default", evalOutside?.LazyAnimal.LazyChild.Name);
        //Equal("Default", evalOutside?.LazyAnimal.LazierChild.Name);
        
        //// Works
        //Equal("Dog", evalOutside?.LazierAnimal.Name);
        //Equal("Dog", evalOutside?.LazierAnimal.LazierChild.Name);
        //Equal("Dog", evalOutside?.LazierAnimal.LazierChild.LazierChild.Name);
        
        //// If lazy again inside it, will not work
        //Equal("Default", evalOutside?.LazierAnimal.LazyChild.Name);
    }

    [Fact]
    public void LazyOutsideClassicFailsClassicOutsideFails()
    {
        Cage? evalOutside;
        using (OverrideService<IAnimal>.Use<AnimalDog>())
        {
            evalOutside = sp.GetService<Cage>();
        }
        
        // Standard lazy fails, for child and grand child
        Equal("Default", evalOutside?.LazyAnimal.Name);
        Equal("Default", evalOutside?.LazyAnimal.LazyChild.Name);
        Equal("Default", evalOutside?.LazyAnimal.LazierChild.Name);
    }

    [Fact]
    public void LazyOutsideNewWorks()
    {
        Cage? evalOutside;
        using (OverrideService<IAnimal>.Use<AnimalDog>())
        {
            evalOutside = sp.GetService<Cage>();
        }
        
        // Works
        Equal("Dog", evalOutside?.LazierAnimal.Name);
        Equal("Dog", evalOutside?.LazierAnimal.LazierChild.Name);
        Equal("Dog", evalOutside?.LazierAnimal.LazierChild.LazierChild.Name);
        
    }

    [Fact]
    public void LazyOutsideNewWithInnerClassicFailsWorks()
    {
        Cage? evalOutside;
        using (OverrideService<IAnimal>.Use<AnimalDog>())
        {
            evalOutside = sp.GetService<Cage>();
        }
        
        // If lazy again inside it, will not work
        Equal("Default", evalOutside?.LazierAnimal.LazyChild.Name);
    }
}
