using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ToSic.Sys.DI.Override;

public class OverrideDiagnostics(IServiceProvider sp)
{
    #region Classes

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

    internal interface ICage;

    internal class Cage : ICage;
    internal class Cage2 : ICage;

    #endregion

    public class Startup() : QuickStartup(services =>
    {
        services.TryAddTransient<AnimalDefault>();
        services.TryAddTransient(OverrideService<IAnimal>.Register<AnimalDefault>());
        services.TryAddTransient<AnimalDog>();
        services.TryAddTransient<AnimalMonkey>();

        services.TryAddTransient<Cage>();
        services.TryAddTransient(OverrideService<ICage>.Register<Cage>());
        services.TryAddTransient<Cage2>();
    });

    [Fact]
    public void Initial_NoDepth()
        => Equal(0, OverrideService<IAnimal>.OverrideCount);
    
    [Fact]
    public void Initial_NoPath()
        => Equal("", OverrideService<IAnimal>.OverridePath);
    
    [Fact]
    public void Using1_Depth1()
    {
        using (OverrideService<IAnimal>.Use<AnimalDog>())
            Equal(1, OverrideService<IAnimal>.OverrideCount);
    }
    
    [Fact]
    public void Using1_Path1()
    {
        using (OverrideService<IAnimal>.Use<AnimalDog>())
            Equal("Using1_Path1", OverrideService<IAnimal>.OverridePath);
    }
    
    [Fact]
    public void Using1_Hinted_Path1()
    {
        using (OverrideService<IAnimal>.Use<AnimalDog>(hint: "Hint"))
            Equal("Using1_Hinted_Path1 (Hint)", OverrideService<IAnimal>.OverridePath);
    }
    
    [Fact]
    public void Using1_NoSkip_Depth1()
    {
        using (OverrideService<IAnimal>.UseIfNotOverridden<AnimalDog>())
            Equal(1, OverrideService<IAnimal>.OverrideCount);
    }
    
    [Fact]
    public void Using1_NoSkip_Path1()
    {
        using (OverrideService<IAnimal>.UseIfNotOverridden<AnimalDog>())
            Equal("Using1_NoSkip_Path1", OverrideService<IAnimal>.OverridePath);
    }
    
    [Fact]
    public void Using2_Depth2()
    {
        using (OverrideService<IAnimal>.Use<AnimalDog>())
            using (OverrideService<IAnimal>.Use<AnimalMonkey>())
                Equal(2, OverrideService<IAnimal>.OverrideCount);
    }
    
    [Fact]
    public void Using2_Path2()
    {
        using (OverrideService<IAnimal>.Use<AnimalDog>())
            using (OverrideService<IAnimal>.Use<AnimalMonkey>())
                Equal("Using2_Path2 > Using2_Path2", OverrideService<IAnimal>.OverridePath);
    }
    
    [Fact]
    public void Using2_Hinted_Path2()
    {
        using (OverrideService<IAnimal>.Use<AnimalDog>(hint: "dog"))
            using (OverrideService<IAnimal>.Use<AnimalMonkey>(hint: "monkey"))
                Equal("Using2_Hinted_Path2 (dog) > Using2_Hinted_Path2 (monkey)", OverrideService<IAnimal>.OverridePath);
    }
    
    [Fact]
    public void Using2_Skipped_Depth1()
    {
        using (OverrideService<IAnimal>.Use<AnimalDog>())
            using (OverrideService<IAnimal>.UseIfNotOverridden<AnimalMonkey>())
                Equal(1, OverrideService<IAnimal>.OverrideCount);
    }
    
    [Fact]
    public void Using1_1_Parent_Depth1()
    {
        using (OverrideService<IAnimal>.Use<AnimalDog>())
            using (OverrideService<ICage>.Use<Cage2>())
                Equal(1, OverrideService<IAnimal>.OverrideCount);
    }
    
    [Fact]
    public void Using1_1_Parent_Path1()
    {
        using (OverrideService<IAnimal>.Use<AnimalDog>())
            using (OverrideService<ICage>.Use<Cage2>())
                Equal("Using1_1_Parent_Path1", OverrideService<IAnimal>.OverridePath);
    }
    
    [Fact]
    public void Using1_1_Child_Depth1()
    {
        using (OverrideService<IAnimal>.Use<AnimalDog>())
            using (OverrideService<ICage>.Use<Cage2>())
                Equal(1, OverrideService<ICage>.OverrideCount);
    }
    
    [Fact]
    public void Using1_1_Child_Path1()
    {
        using (OverrideService<IAnimal>.Use<AnimalDog>())
            using (OverrideService<ICage>.Use<Cage2>())
                Equal("Using1_1_Child_Path1", OverrideService<ICage>.OverridePath);
    }
    
    [Fact]
    public void Using1_Exit_Depth0()
    {
        using (OverrideService<IAnimal>.Use<AnimalDog>())
        { }
        Equal(0, OverrideService<IAnimal>.OverrideCount);
    }

}
