using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ToSic.Sys.Run.Startup;


namespace ToSic.Sys.DI.Override;

public class OverrideGeneratorWithOptions(ISysServiceProvider sp)
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

    private abstract class Animal(Lazy<IAnimal> al, LazySvc<IAnimal> als, GeneratorBasic<IAnimal> genPupC, Generator<IAnimal> getPupS) : IAnimal
    {
        public string Name => GetType().Name;
        
        public IAnimal LazyChild => al.Value;
        public IAnimal SuperLazyChild => als.Value;
        public IAnimal BirthLazy() => genPupC.New();
        public IAnimal BirthSuper() => getPupS.New();
    }

    private class AnimalDefault(Lazy<IAnimal> al, LazySvc<IAnimal> als, GeneratorBasic<IAnimal> genPupC, Generator<IAnimal> getPupS) : Animal(al, als, genPupC, getPupS);
    private class AnimalDog(Lazy<IAnimal> al, LazySvc<IAnimal> als, GeneratorBasic<IAnimal> genPupC, Generator<IAnimal> getPupS) : Animal(al, als, genPupC, getPupS);
    private class AnimalMonkey(Lazy<IAnimal> al, LazySvc<IAnimal> als, GeneratorBasic<IAnimal> genPupC, Generator<IAnimal> getPupS) : Animal(al, als, genPupC, getPupS);

    internal class CustomizableCage(Lazy<IAnimal> animal, LazySvc<IAnimal> aniLz, GeneratorBasic<IAnimal> aniGen, Generator<IAnimal> aniGenS): IServiceWithSetup<CustomizableCage.Options>
    {
        internal record Options(string Color);
        private Options _opts = null!;
        public void Setup(Options options) => _opts = options;
        public string Color => _opts?.Color ?? "Missing";
        public IAnimal LazyAnimal => animal.Value;
        public IAnimal SuperLazyAnimal => aniLz.Value;
        public IAnimal GenerateAnimal() => aniGen.New();
        public IAnimal GenerateAnimalSuper() => aniGenS.New();
    }

    #endregion



    #region Startup / Service Registration

    public class Startup() : QuickStartup(services =>
    {
        // Add Lazy and Super-Generators
        services.AddSysCoreDi();

        // Generator - now
        services.TryAddTransient(typeof(GeneratorBasic<>));      // Classic generator
        services.TryAddTransient(typeof(GeneratorBasic<,>));     // Classic Generator with Options

        // Register the default animal, and the default service for IAnimal
        services.TryAddTransient<AnimalDefault>();
        services.TryAddTransient(OverrideService<IAnimal>.Register<AnimalDefault>());

        // Register the remaining animals, so we can override them in the tests
        services.TryAddTransient<AnimalDog>();
        services.TryAddTransient<AnimalMonkey>();

        services.TryAddTransient<CustomizableCage>();
    });

    #endregion




    #region Generator

    [Fact]
    public void GeneratorInsideAllVariantsWork()
    {
        using (OverrideService<IAnimal>.Use<AnimalDog>())
        {
            var evalInside = sp.GetRequiredService<Generator<CustomizableCage, CustomizableCage.Options>>();
            Equal("Red", evalInside.New(new("Red")).Color);
            Equal(nameof(AnimalDog), evalInside.New(new("Red")).LazyAnimal.Name);
        }
    }

    [Fact]
    public void GeneratorInsideAllVariantsRepeatingWorks()
    {
        using (OverrideService<IAnimal>.Use<AnimalDog>())
        {
            var evalInside = sp.GetRequiredService<Generator<CustomizableCage, CustomizableCage.Options>>();
            for (var i = 0; i < 5; i++)
            {
                Equal("Red", evalInside.New(new("Red")).Color);
                Equal("Green", evalInside.New(new("Green")).Color);
            }
        }
    }

    [Fact]
    public void GeneratorMainOutsideClassicUsedToFailNowWorks()
    {
        Generator<CustomizableCage, CustomizableCage.Options> evalOutside;
        using (OverrideService<IAnimal>.Use<AnimalMonkey>())
        {
            evalOutside = sp.GetRequiredService<Generator<CustomizableCage, CustomizableCage.Options>>();
        }

        // Standard lazy fails, for child and grand child
        Equal("Red", evalOutside.New(new("Red")).Color);
        Equal(nameof(AnimalMonkey), evalOutside.New(new("Red")).LazyAnimal.Name);

    }
    [Fact]
    public void GeneratorMainOutsideValueUnknown()
    {
        Generator<CustomizableCage, CustomizableCage.Options> evalOutside;
        var monkey = sp.GetRequiredService<AnimalMonkey>();
        using (OverrideService<IAnimal>.Use(monkey))
        {
            evalOutside = sp
                .GetRequiredService<ISysServiceProvider>()
                .GetRequiredService<Generator<CustomizableCage, CustomizableCage.Options>>();
        }

        // Standard lazy fails, for child and grand child
        Equal(nameof(AnimalMonkey), evalOutside.New(new("Red")).LazyAnimal.Name);

    }

    //[Fact]
    //public void GeneratorMainOutsideSuperWorks()
    //{
    //    GeneratorSo<IAnimal> evalOutside;
    //    using (OverrideService<IAnimal>.Use<AnimalMonkey>())
    //    {
    //        evalOutside = sp.GetService<GeneratorSo<IAnimal>>()!;
    //    }

    //    // Standard lazy fails, for child and grand child
    //    Equal(nameof(AnimalMonkey), evalOutside.New().Name);
    //    Equal(nameof(AnimalMonkey), evalOutside.New().Name);
    //    Equal(nameof(AnimalMonkey), evalOutside.New().SuperLazyChild.Name);
    //    Equal(nameof(AnimalMonkey), evalOutside.New().SuperLazyChild.Name);
    //    Equal(nameof(AnimalMonkey), evalOutside.New().BirthSuper().Name);
    //    Equal(nameof(AnimalMonkey), evalOutside.New().BirthSuper().Name);
    //}

    //[Fact]
    //public void GeneratorOutsideClassicFails()
    //{
    //    Cage? evalOutside;
    //    using (OverrideService<IAnimal>.Use<AnimalDog>())
    //    {
    //        evalOutside = sp.GetService<Cage>();
    //    }

    //    // Standard lazy fails, for child and grand child
    //    Equal(NameIfLazyFails, evalOutside?.GenerateAnimal().Name);
    //    Equal(NameIfLazyFails, evalOutside?.GenerateAnimal().BirthLazy().Name);
    //    Equal(NameIfLazyFails, evalOutside?.GenerateAnimal().BirthLazy().Name);
    //}

    //[Fact]
    //public void GeneratorOutsideSuperWorks()
    //{
    //    Cage? evalOutside;
    //    using (OverrideService<IAnimal>.Use<AnimalDog>())
    //    {
    //        evalOutside = sp.GetService<Cage>();
    //    }

    //    // Works
    //    Equal(nameof(AnimalDog), evalOutside?.GenerateAnimalSuper().Name);
    //    Equal(nameof(AnimalDog), evalOutside?.GenerateAnimalSuper().Name);
    //    Equal(nameof(AnimalDog), evalOutside?.GenerateAnimalSuper().BirthSuper().Name);
    //    Equal(nameof(AnimalDog), evalOutside?.GenerateAnimalSuper().BirthSuper().BirthSuper().Name);
    //}

    //[Fact]
    //public void GeneratorOutsideWithInnerClassicFails()
    //{
    //    Cage? evalOutside;
    //    using (OverrideService<IAnimal>.Use<AnimalDog>())
    //    {
    //        evalOutside = sp.GetService<Cage>();
    //    }

    //    // If lazy again deep down, that level will not work
    //    Equal(NameIfLazyFails, evalOutside?.GenerateAnimalSuper().BirthLazy().Name);
    //    Equal(NameIfLazyFails, evalOutside?.GenerateAnimalSuper().LazyChild.Name);
    //}

    #endregion



}
