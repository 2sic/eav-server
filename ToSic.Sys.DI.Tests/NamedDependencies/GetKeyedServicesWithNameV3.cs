using Microsoft.Extensions.DependencyInjection;
using ToSic.Sys.DI;
using ToSic.Sys.Run.Startup;

// ReSharper disable ClassNeverInstantiated.Local


namespace ToSic.NamedDependencies;

public class GetKeyedServicesWithNameV3
{
    private interface INamedService;

    private class NamedJohn : INamedService;

    private class NamedJoe : INamedService;

    private class NamedMarc : INamedService;

    private class KeyedRegistryUser(IAllKeyedServices<string, INamedService> services)
    {
        public int Count => services.Count();

        public IEnumerable<KeyedService<string, INamedService>> Services => services;
    }

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
        sc.AddSysCoreDi();
        sc.AddKeyedTransient<INamedService, NamedJohn>(nameof(NamedJohn));
        sc.AddKeyedTransient<INamedService, NamedJoe>(nameof(NamedJoe));
        sc.AddKeyedServiceRegistry();
        // this one won't get discovered, because order matters
        sc.AddKeyedTransient<INamedService, NamedMarc>(nameof(NamedMarc));
        sc.AddTransient<KeyedRegistryUser>();
        var sp = sc.BuildServiceProvider();
        return sp;
    }

    [Fact]
    public void Add3_Has3()
        => Equal(3, BuildServiceProviderWithNamed().GetRequiredService<IAllKeyedServices<string, INamedService>>().Count());

    [Fact]
    public void Add3_NamesMatch()
        => Equal(
            [nameof(NamedJohn), nameof(NamedJoe), nameof(NamedMarc)],
            BuildServiceProviderWithNamed()
                .GetRequiredService<IAllKeyedServices<string, INamedService>>()
                .Select(k => k.Key));

    [Fact]
    public void Add3_JohnMatches()
        => IsType<NamedJohn>(
            BuildServiceProviderWithNamed()
                .GetRequiredService<IAllKeyedServices<string, INamedService>>()
                .First(pair => pair.Key == nameof(NamedJohn))
                .Service
        );

    [Fact]
    public void KeyedUserConstructorWorks()
        => Equal(3, BuildServiceProviderWithNamed().GetRequiredService<KeyedRegistryUser>().Count);

    [Fact]
    public void KeyedUserServicesWorks()
        => Equal(
            [nameof(NamedJohn), nameof(NamedJoe), nameof(NamedMarc)],
            BuildServiceProviderWithNamed().GetRequiredService<KeyedRegistryUser>().Services
                .Select(s => s.Key));
}
