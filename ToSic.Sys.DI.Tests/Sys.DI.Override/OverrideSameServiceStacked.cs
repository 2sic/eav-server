using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ToSic.Sys.DI.Override;

public class OverrideSameServiceStacked(IServiceProvider sp)
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
        services.TryAddTransient<AnimalDefault>();
        services.TryAddTransient(OverrideService<IAnimal>.Register<AnimalDefault>());

        services.TryAddTransient<Cage>();
        services.TryAddTransient<AnimalDog>();
        services.TryAddTransient<AnimalMonkey>();
        services.TryAddTransient<AnimalElephant>();
        services.TryAddTransient<AnimalBird>();
    });

    [Fact]
    public void OverrideStacked_X2()
    {
        // Level 0 before
        Equal("Default", sp.GetRequiredService<Cage>().Animal.Name);

        using (OverrideService<IAnimal>.Use<AnimalDog>())
        {
            // Level 1 before
            Equal("Dog", sp.GetRequiredService<Cage>().Animal.Name);

            using (OverrideService<IAnimal>.Use<AnimalMonkey>())
            {
                // Level 2
                Equal("Monkey", sp.GetRequiredService<Cage>().Animal.Name);
            }

            // Level 1 after
            Equal("Dog", sp.GetRequiredService<Cage>().Animal.Name);
        }

        // Level 0 after
        Equal("Default", sp.GetRequiredService<Cage>().Animal.Name);
    }

    [Fact]
    public void OverrideStacked_X3()
    {
        // Level 0 before
        Equal("Default", sp.GetRequiredService<Cage>().Animal.Name);

        using (OverrideService<IAnimal>.Use<AnimalDog>())
        {
            // Level 1 before
            Equal("Dog", sp.GetRequiredService<Cage>().Animal.Name);

            using (OverrideService<IAnimal>.Use<AnimalMonkey>())
            {
                // Level 2 before
                Equal("Monkey", sp.GetRequiredService<Cage>().Animal.Name);

                using (OverrideService<IAnimal>.Use<AnimalElephant>())
                {
                    // Level 3 before
                    Equal("Elephant", sp.GetRequiredService<Cage>().Animal.Name);
                }

                // Level 2 after
                Equal("Monkey", sp.GetRequiredService<Cage>().Animal.Name);
            }

            // Level 1 after
            Equal("Dog", sp.GetRequiredService<Cage>().Animal.Name);
        }

        // Level 0 after
        Equal("Default", sp.GetRequiredService<Cage>().Animal.Name);
    }

    [Fact]
    public void OverrideStacked_X4()
    {
        // Level 0 before
        Equal("Default", sp.GetRequiredService<Cage>().Animal.Name);

        using (OverrideService<IAnimal>.Use<AnimalDog>())
        {
            // Level 1 before
            Equal("Dog", sp.GetRequiredService<Cage>().Animal.Name);

            using (OverrideService<IAnimal>.Use<AnimalMonkey>())
            {
                // Level 2 before
                Equal("Monkey", sp.GetRequiredService<Cage>().Animal.Name);

                using (OverrideService<IAnimal>.Use<AnimalElephant>())
                {
                    // Level 3 before
                    Equal("Elephant", sp.GetRequiredService<Cage>().Animal.Name);

                    using (OverrideService<IAnimal>.Use<AnimalBird>())
                    {
                        // Level 4 before
                        Equal("Bird", sp.GetRequiredService<Cage>().Animal.Name);
                    }

                    // Level 3 after
                    Equal("Elephant", sp.GetRequiredService<Cage>().Animal.Name);
                }


                // Level 2 after
                Equal("Monkey", sp.GetRequiredService<Cage>().Animal.Name);
            }

            // Level 1 after
            Equal("Dog", sp.GetRequiredService<Cage>().Animal.Name);
        }

        // Level 0 after
        Equal("Default", sp.GetRequiredService<Cage>().Animal.Name);
    }

}