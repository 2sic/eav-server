using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ToSic.Sys.DI;
// ReSharper disable ClassNeverInstantiated.Local


namespace ToSic.NamedDependencies;

public class GetKeyedServicesWithNameV2
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
        sc.AddKeyedTransient<INamedService, NamedJohn>(nameof(NamedJohn));
        sc.AddKeyedTransient<INamedService, NamedJoe>(nameof(NamedJoe));
        sc.AddKeyedServiceEnumeration<string, INamedService>();
        var sp = sc.BuildServiceProvider();
        return sp;
    }

    [Fact]
    public void Add2_Has2()
        => Equal(2, BuildServiceProviderWithNamed().GetRequiredService<IEnumerable<KeyedService<string, INamedService>>>().Count());

    [Fact]
    public void Add2_NamesMatch()
        => Equal(
            [nameof(NamedJohn), nameof(NamedJoe)],
            BuildServiceProviderWithNamed()
                .GetRequiredService<IEnumerable<KeyedService<string, INamedService>>>()
                .Select(k => k.Key));

    [Fact]
    public void Add2_JohnMatches()
        => IsType<NamedJohn>(
            BuildServiceProviderWithNamed()
                .GetRequiredService<IEnumerable<KeyedService<string, INamedService>>>()
                .First(pair => pair.Key == nameof(NamedJohn))
                .Service
        );
}
