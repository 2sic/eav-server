using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ToSic.Sys.DI.Override;

public class OverrideMultipleServices(IServiceProvider sp)
{
    #region Test Classes

    internal abstract class Animal
    {
        public string Name => GetType().Name.Substring("Animal".Length);
    }

    private class AnimalDefault : Animal;

    private class AnimalDog : Animal;

    private class AnimalMonkey : Animal;

    internal abstract class Material
    {
        public string Name => GetType().Name.Substring("Material".Length);
    }

    private class MaterialDefault : Material;

    private class MaterialSteel : Material;

    private class MaterialWood : Material;


    internal class Cage(Animal animal, Material material)
    {
        public Animal Animal { get; } = animal;
        public Material Material { get; } = material;
    }

    #endregion

    public class Startup() : QuickStartup(services =>
    {
        services.TryAddTransient<Cage>();

        services.TryAddTransient<AnimalDefault>();
        services.TryAddTransient(OverrideService<Animal>.Register<AnimalDefault>());
        services.TryAddTransient<AnimalDog>();
        services.TryAddTransient<AnimalMonkey>();

        services.TryAddTransient<MaterialDefault>();
        services.TryAddTransient(OverrideService<Material>.Register<MaterialDefault>());
        services.TryAddTransient<MaterialSteel>();
        services.TryAddTransient<MaterialWood>();
    });

    private void VerifyAnimalAndMaterial(string expectedAnimal, string expectedMaterial)
    {
        var cage = sp.GetRequiredService<Cage>();
        Equal(expectedAnimal, cage.Animal.Name);
        Equal(expectedMaterial, cage.Material.Name);
    }
    
    [Fact]
    public void OverrideStacked_X2()
    {
        // Level 0 before
        VerifyAnimalAndMaterial("Default", "Default");
        
        using (OverrideService<Animal>.Use<AnimalDog>())
        using (OverrideService<Material>.Use<MaterialSteel>())
        {
            // Level 1 before
            VerifyAnimalAndMaterial("Dog", "Steel");

            using (OverrideService<Animal>.Use<AnimalMonkey>())
            {
                // Level 2
                VerifyAnimalAndMaterial("Monkey", "Steel");
                using (OverrideService<Material>.Use<MaterialWood>())
                {
                    // Level 2 inner
                    VerifyAnimalAndMaterial("Monkey", "Wood");
                }
                VerifyAnimalAndMaterial("Monkey", "Steel");
            }

            // Level 1 after
            VerifyAnimalAndMaterial("Dog", "Steel");
        }

        // Level 0 after
        VerifyAnimalAndMaterial("Default", "Default");
    }

    //[Fact]
    //public void OverrideStacked_X3()
    //{
    //    using (OverrideService<Animal>.Use<AnimalDog>())
    //    {
    //        // Level 1 before
    //        Equal("Dog", sp.GetRequiredService<Cage>().Animal.Name);

    //        using (OverrideService<Animal>.Use<AnimalMonkey>())
    //        {
    //            // Level 2 before
    //            Equal("Monkey", sp.GetRequiredService<Cage>().Animal.Name);

    //            using (OverrideService<Animal>.Use<AnimalElephant>())
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
    //}

    //[Fact]
    //public void OverrideStacked_X4()
    //{
    //    using (OverrideService<Animal>.Use<AnimalDog>())
    //    {
    //        // Level 1 before
    //        Equal("Dog", sp.GetRequiredService<Cage>().Animal.Name);

    //        using (OverrideService<Animal>.Use<AnimalMonkey>())
    //        {
    //            // Level 2 before
    //            Equal("Monkey", sp.GetRequiredService<Cage>().Animal.Name);

    //            using (OverrideService<Animal>.Use<AnimalElephant>())
    //            {
    //                // Level 3 before
    //                Equal("Elephant", sp.GetRequiredService<Cage>().Animal.Name);

    //                using (OverrideService<Animal>.Use<AnimalBird>())
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
    //}


}