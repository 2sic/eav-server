using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ToSic.Sys.Run.Startup;

namespace ToSic.Sys.DI.Override;

public class OverrideSameServiceStackedCtx(ISysServiceProvider sp)
{
    #region Test Classes

    internal interface IAnimal
    {
        string Name { get; }
    }
    
    private abstract class Animal : IAnimal
    {
        public string Name => GetType().Name.Substring("Animal".Length);
    }
    
    private class AnimalDefault : Animal;
    private class AnimalDog : Animal;
    private class AnimalMonkey : Animal;
    private class AnimalElephant : Animal;
    private class AnimalBird : Animal;

    internal class Cage(IAnimal animal)
    {
        public IAnimal Animal { get; } = animal;
    }
    
    #endregion

    public class Startup() : QuickStartup(services =>
    {
        services
            .AddSysCoreDiServiceProviderTac()
            .AddSysCoreDiContextTac()
            .AddSysCoreDiLazyAndGeneratorsTac();
        services.TryAddTransient<AnimalDefault>();
        services.TryAddTransient(OverrideService<IAnimal>.Register<AnimalDefault>());
        //services.TryAddTransient<IAnimal>(sp =>
        //{
        //    var sysSp = sp.GetRequiredService<ISysServiceProvider>();
        //    return OverrideService<IAnimal>.CurrentFactory?.Invoke(sysSp)
        //           ?? sysSp.GetRequiredService<AnimalDefault>();
        //});

        services.TryAddTransient<Cage>();
        services.TryAddTransient<AnimalDog>();
        services.TryAddTransient<AnimalMonkey>();
        services.TryAddTransient<AnimalElephant>();
        services.TryAddTransient<AnimalBird>();
    });

    [Fact]
    public void OverrideStackedCtx_X2()
    {
        // Level 0 before
        Null(sp.GetRequiredService<Ctx<IAnimal>>().ValueOptional);

        LazySvc<Ctx<IAnimal>> lazyLevel1;

        var injectDog = sp.GetRequiredService<AnimalDog>();
        using (Ctx<IAnimal>.Use(injectDog))
        {
            // Level 1 before
            Equal("Dog", sp.GetRequiredService<Ctx<IAnimal>>().Value.Name);
            lazyLevel1 = sp.GetRequiredService<LazySvc<Ctx<IAnimal>>>();

            var injectMonkey= sp.GetRequiredService<AnimalMonkey>();
            using (Ctx<IAnimal>.Use(injectMonkey))
            {
                // Level 2
                Equal("Monkey", sp.GetRequiredService<Ctx<IAnimal>>().Value.Name);
            }

            // Level 1 after
            Equal("Dog", sp.GetRequiredService<Ctx<IAnimal>>().Value.Name);
        }

        Equal("Dog", lazyLevel1.Value.Value.Name);

        // Level 0 after
        Null(sp.GetRequiredService<Ctx<IAnimal>>().ValueOptional);
    }

    //[Fact]
    //public void OverrideStacked_X3()
    //{
    //    // Level 0 before
    //    Equal("Default", sp.GetRequiredService<Cage>().Animal.Name);

    //    using (OverrideService<IAnimal>.Use<AnimalDog>())
    //    {
    //        // Level 1 before
    //        Equal("Dog", sp.GetRequiredService<Cage>().Animal.Name);

    //        using (OverrideService<IAnimal>.Use<AnimalMonkey>())
    //        {
    //            // Level 2 before
    //            Equal("Monkey", sp.GetRequiredService<Cage>().Animal.Name);

    //            using (OverrideService<IAnimal>.Use<AnimalElephant>())
    //            {
    //                // Level 3 before
    //                Equal("Elephant", sp.GetRequiredService<Cage>().Animal.Name);
    //            }

    //            // Level 2 after
    //            Equal("Monkey", sp.GetRequiredService<Cage>().Animal.Name);
    //        }

    //        // Level 1 after
    //        Equal("Dog", sp.GetRequiredService<Cage>().Animal.Name);
    //    }

    //    // Level 0 after
    //    Equal("Default", sp.GetRequiredService<Cage>().Animal.Name);
    //}

    //[Fact]
    //public void OverrideStacked_X4()
    //{
    //    // Level 0 before
    //    Equal("Default", sp.GetRequiredService<Cage>().Animal.Name);

    //    using (OverrideService<IAnimal>.Use<AnimalDog>())
    //    {
    //        // Level 1 before
    //        Equal("Dog", sp.GetRequiredService<Cage>().Animal.Name);

    //        using (OverrideService<IAnimal>.Use<AnimalMonkey>())
    //        {
    //            // Level 2 before
    //            Equal("Monkey", sp.GetRequiredService<Cage>().Animal.Name);

    //            using (OverrideService<IAnimal>.Use<AnimalElephant>())
    //            {
    //                // Level 3 before
    //                Equal("Elephant", sp.GetRequiredService<Cage>().Animal.Name);

    //                using (OverrideService<IAnimal>.Use<AnimalBird>())
    //                {
    //                    // Level 4 before
    //                    Equal("Bird", sp.GetRequiredService<Cage>().Animal.Name);
    //                }

    //                // Level 3 after
    //                Equal("Elephant", sp.GetRequiredService<Cage>().Animal.Name);
    //            }


    //            // Level 2 after
    //            Equal("Monkey", sp.GetRequiredService<Cage>().Animal.Name);
    //        }

    //        // Level 1 after
    //        Equal("Dog", sp.GetRequiredService<Cage>().Animal.Name);
    //    }

    //    // Level 0 after
    //    Equal("Default", sp.GetRequiredService<Cage>().Animal.Name);
    //}

}