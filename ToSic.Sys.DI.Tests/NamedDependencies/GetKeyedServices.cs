using Microsoft.Extensions.DependencyInjection;
using ToSic.Sys.DI;
// ReSharper disable ClassNeverInstantiated.Local


namespace ToSic.NamedDependencies;

public class GetKeyedServices
{
    private interface INamedService;

    private class NamedJohn : INamedService;

    private class NamedJoe : INamedService;

    [Fact]
    public void NoRegistrations_HasNone()
    {
        var sp = new ServiceCollection()
            .BuildServiceProvider();

        // If no requirement checkers are registered, GetAllKeysForService should return an empty enumerable.
        Empty(sp.GetAllKeysForService<INamedService>());
    }


    private static ServiceProvider BuildServiceProviderWithNamed()
    {
        var sc = new ServiceCollection();
        sc.TryAddKeyedTransientWithMarker<INamedService, NamedJohn>(nameof(NamedJohn));
        sc.TryAddKeyedTransientWithMarker<INamedService, NamedJoe>(nameof(NamedJoe));
        var sp = sc.BuildServiceProvider();
        return sp;
    }

    [Fact]
    public void Add2_Has2()
        => Equal(2, BuildServiceProviderWithNamed().GetAllKeysForService<INamedService>().Count());

    [Fact]
    public void Add2_NamesMatch()
        => Equal([nameof(NamedJohn), nameof(NamedJoe)], BuildServiceProviderWithNamed().GetAllKeysForService<INamedService>());

    [Fact]
    public void Add2_JohnMatches()
        => IsType<NamedJohn>(
            BuildServiceProviderWithNamed()
                .GetAllKeyedServices<INamedService>()
                .First(pair => pair.Key == nameof(NamedJohn)).Value
        );

}