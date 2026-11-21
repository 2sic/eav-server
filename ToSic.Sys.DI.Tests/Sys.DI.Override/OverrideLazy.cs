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
        IAnimal LazyChild { get; }
        IAnimal SuperLazyChild { get; }
        IAnimal BirthLazy();
        IAnimal BirthSuper();
    }

    private abstract class Animal(Lazy<IAnimal> al, LazySvc<IAnimal> als, Generator<IAnimal> genPupC, GeneratorSo<IAnimal> getPupS) : IAnimal
    {
        public string Name => GetType().Name;
        
        public IAnimal LazyChild => al.Value;
        public IAnimal SuperLazyChild => als.Value;
        public IAnimal BirthLazy() => genPupC.New();
        public IAnimal BirthSuper() => getPupS.New();
    }

    private class AnimalDefault(Lazy<IAnimal> al, LazySvc<IAnimal> als, Generator<IAnimal> genPupC, GeneratorSo<IAnimal> getPupS) : Animal(al, als, genPupC, getPupS);
    private class AnimalDog(Lazy<IAnimal> al, LazySvc<IAnimal> als, Generator<IAnimal> genPupC, GeneratorSo<IAnimal> getPupS) : Animal(al, als, genPupC, getPupS);
    private class AnimalMonkey(Lazy<IAnimal> al, LazySvc<IAnimal> als, Generator<IAnimal> genPupC, GeneratorSo<IAnimal> getPupS) : Animal(al, als, genPupC, getPupS);
    private class AnimalElephant(Lazy<IAnimal> al, LazySvc<IAnimal> als, Generator<IAnimal> genPupC, GeneratorSo<IAnimal> getPupS) : Animal(al, als, genPupC, getPupS);
    private class AnimalBird(Lazy<IAnimal> al, LazySvc<IAnimal> als, Generator<IAnimal> genPupC, GeneratorSo<IAnimal> getPupS) : Animal(al, als, genPupC, getPupS);

    internal class Cage(Lazy<IAnimal> animal, LazySvc<IAnimal> aniLz, Generator<IAnimal> aniGen, GeneratorSo<IAnimal> aniGenS)
    {
        public IAnimal LazyAnimal => animal.Value;
        public IAnimal SuperLazyAnimal => aniLz.Value;
        public IAnimal GenerateAnimal() => aniGen.New();
        public IAnimal GenerateAnimalSuper() => aniGenS.New();
    }

    internal class CustomizableCage: IServiceWithSetup<CustomizableCage.Options>
    {
        internal record Options(string Color);
        private Options _opts = null!;
        public void Setup(Options options) => _opts = options;
        public string Color => _opts?.Color ?? "Missing";
    }

    #endregion



    #region Startup / Service Registration

    public class Startup() : QuickStartup(services =>
    {
        // Lazy - classic and super
        services.TryAddTransient(typeof(Lazy<>), typeof(LazyImplementation<>));
        services.TryAddTransient(typeof(LazySvc<>));

        // Generator - now
        services.TryAddTransient(typeof(Generator<>));      // Classic generator
        services.TryAddTransient(typeof(Generator<,>));     // Classic Generator with Options
        services.TryAddTransient(typeof(GeneratorSo<>));    // Super generator
        services.TryAddTransient(typeof(GeneratorSo<,>));   // Super Generator with Options

        // Register the default animal, and the default service for IAnimal
        services.TryAddTransient<AnimalDefault>();
        services.TryAddTransient(OverrideService<IAnimal>.Register<AnimalDefault>());

        // Register the remaining animals, so we can override them in the tests
        services.TryAddTransient<Cage>();
        services.TryAddTransient<AnimalDog>();
        services.TryAddTransient<AnimalMonkey>();
        services.TryAddTransient<AnimalElephant>();
        services.TryAddTransient<AnimalBird>();

        services.TryAddTransient<CustomizableCage>();
    });

    #endregion



    #region Lazy

    [Fact]
    public void LazyInsideAllVariantsWork()
    {
        using (OverrideService<IAnimal>.Use<AnimalDog>())
        {
            var evalInside = sp.GetRequiredService<Cage>();
            Equal(nameof(AnimalDog), evalInside.LazyAnimal.Name);
            Equal(nameof(AnimalDog), evalInside.SuperLazyAnimal.Name);
            
            Equal(nameof(AnimalDog), evalInside.LazyAnimal.LazyChild.Name);
            Equal(nameof(AnimalDog), evalInside.LazyAnimal.SuperLazyChild.Name);

            Equal(nameof(AnimalDog), evalInside.SuperLazyAnimal.LazyChild.Name);
            Equal(nameof(AnimalDog), evalInside.SuperLazyAnimal.SuperLazyChild.Name);
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
        Equal(NameIfLazyFails, evalOutside?.LazyAnimal.Name);
        Equal(NameIfLazyFails, evalOutside?.LazyAnimal.LazyChild.Name);
        Equal(NameIfLazyFails, evalOutside?.LazyAnimal.SuperLazyChild.Name);
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
        Equal(NameIfLazyFails, evalOutside?.SuperLazyAnimal.LazyChild.Name);
    }

    #endregion


    #region Generator

    [Fact]
    public void GeneratorInsideAllVariantsWork()
    {
        using (OverrideService<IAnimal>.Use<AnimalDog>())
        {
            var evalInside = sp.GetRequiredService<Cage>();
            Equal(nameof(AnimalDog), evalInside.GenerateAnimal().Name);
            Equal(nameof(AnimalDog), evalInside.GenerateAnimalSuper().Name);

            Equal(nameof(AnimalDog), evalInside.GenerateAnimal().BirthLazy().Name);
            Equal(nameof(AnimalDog), evalInside.GenerateAnimal().BirthSuper().Name);

            Equal(nameof(AnimalDog), evalInside.GenerateAnimalSuper().BirthLazy().Name);
            Equal(nameof(AnimalDog), evalInside.GenerateAnimalSuper().BirthSuper().Name);
        }
    }

    [Fact]
    public void GeneratorInsideAllVariantsRepeatingWorks()
    {
        using (OverrideService<IAnimal>.Use<AnimalDog>())
        {
            var evalInside = sp.GetRequiredService<Cage>();
            for (var i = 0; i < 5; i++)
            {
                Equal(nameof(AnimalDog), evalInside.GenerateAnimal().Name);
                Equal(nameof(AnimalDog), evalInside.GenerateAnimalSuper().Name);

                Equal(nameof(AnimalDog), evalInside.GenerateAnimal().BirthLazy().Name);
                Equal(nameof(AnimalDog), evalInside.GenerateAnimal().BirthSuper().Name);

                Equal(nameof(AnimalDog), evalInside.GenerateAnimalSuper().BirthLazy().Name);
                Equal(nameof(AnimalDog), evalInside.GenerateAnimalSuper().BirthSuper().Name);
            }
        }
    }

    [Fact]
    public void GeneratorMainOutsideClassicFails()
    {
        Generator<IAnimal> evalOutside;
        using (OverrideService<IAnimal>.Use<AnimalMonkey>())
        {
            evalOutside = sp.GetService<Generator<IAnimal>>()!;
        }

        // Standard lazy fails, for child and grand child
        Equal(NameIfLazyFails, evalOutside.New().Name);
    }

    [Fact]
    public void GeneratorMainOutsideSuperWorks()
    {
        GeneratorSo<IAnimal> evalOutside;
        using (OverrideService<IAnimal>.Use<AnimalMonkey>())
        {
            evalOutside = sp.GetService<GeneratorSo<IAnimal>>()!;
        }

        // Standard lazy fails, for child and grand child
        Equal(nameof(AnimalMonkey), evalOutside.New().Name);
        Equal(nameof(AnimalMonkey), evalOutside.New().Name);
        Equal(nameof(AnimalMonkey), evalOutside.New().SuperLazyChild.Name);
        Equal(nameof(AnimalMonkey), evalOutside.New().SuperLazyChild.Name);
        Equal(nameof(AnimalMonkey), evalOutside.New().BirthSuper().Name);
        Equal(nameof(AnimalMonkey), evalOutside.New().BirthSuper().Name);
    }

    [Fact]
    public void GeneratorOutsideClassicFails()
    {
        Cage? evalOutside;
        using (OverrideService<IAnimal>.Use<AnimalDog>())
        {
            evalOutside = sp.GetService<Cage>();
        }

        // Standard lazy fails, for child and grand child
        Equal(NameIfLazyFails, evalOutside?.GenerateAnimal().Name);
        Equal(NameIfLazyFails, evalOutside?.GenerateAnimal().BirthLazy().Name);
        Equal(NameIfLazyFails, evalOutside?.GenerateAnimal().BirthLazy().Name);
    }

    [Fact]
    public void GeneratorOutsideSuperWorks()
    {
        Cage? evalOutside;
        using (OverrideService<IAnimal>.Use<AnimalDog>())
        {
            evalOutside = sp.GetService<Cage>();
        }

        // Works
        Equal(nameof(AnimalDog), evalOutside?.GenerateAnimalSuper().Name);
        Equal(nameof(AnimalDog), evalOutside?.GenerateAnimalSuper().Name);
        Equal(nameof(AnimalDog), evalOutside?.GenerateAnimalSuper().BirthSuper().Name);
        Equal(nameof(AnimalDog), evalOutside?.GenerateAnimalSuper().BirthSuper().BirthSuper().Name);
    }

    [Fact]
    public void GeneratorOutsideWithInnerClassicFails()
    {
        Cage? evalOutside;
        using (OverrideService<IAnimal>.Use<AnimalDog>())
        {
            evalOutside = sp.GetService<Cage>();
        }

        // If lazy again deep down, that level will not work
        Equal(NameIfLazyFails, evalOutside?.GenerateAnimalSuper().BirthLazy().Name);
        Equal(NameIfLazyFails, evalOutside?.GenerateAnimalSuper().LazyChild.Name);
    }

    #endregion



    #region Generator and Lazy Mixed

    [Fact]
    public void GeneratorAndLazyOutsideSuperWorks()
    {
        Cage? evalOutside;
        using (OverrideService<IAnimal>.Use<AnimalDog>())
        {
            evalOutside = sp.GetService<Cage>();
        }

        // Works
        Equal(nameof(AnimalDog), evalOutside?.GenerateAnimalSuper().Name);
        Equal(nameof(AnimalDog), evalOutside?.GenerateAnimalSuper().SuperLazyChild.Name);
        Equal(nameof(AnimalDog), evalOutside?.GenerateAnimalSuper().SuperLazyChild.SuperLazyChild.Name);
    }


    #endregion
}
