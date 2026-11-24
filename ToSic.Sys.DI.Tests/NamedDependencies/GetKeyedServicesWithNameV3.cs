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

        public IEnumerable<KeyValuePair<string, INamedService>> Services => services;
    }

    [Fact]
    public void NoRegistrations_HasNone()
    {
        var sp = new ServiceCollection()
            .AddSysCoreDiServiceProviderTac()
            .AddSysCoreDiKeyRegistryTac()
            .BuildServiceProvider();

        // If no requirement checkers are registered, GetAllKeysForService should return an empty enumerable.
        Empty(sp.GetService<IAllKeyedServices<string, INamedService>>()!);
    }


    private static ServiceProvider BuildServiceProviderWithNamed()
    {
        var sc = new ServiceCollection()
            .AddSysCoreDiServiceProviderTac()
            .AddSysCoreDiKeyRegistryTac()
            .AddKeyedTransient<INamedService, NamedJohn>(nameof(NamedJohn))
            .AddKeyedTransient<INamedService, NamedJoe>(nameof(NamedJoe))
            // this one won't get discovered, because order matters
            .AddKeyedTransient<INamedService, NamedMarc>(nameof(NamedMarc))
            .AddTransient<KeyedRegistryUser>();
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
                .Value
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
