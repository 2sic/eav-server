using Microsoft.Extensions.DependencyInjection;
using ToSic.Eav.Data.Processing;
using ToSic.Eav.Run.Startup;

namespace ToSic.Eav.Data.Build.Startup;

public class StartupEavDataBuildDataProcessorsTests
{
    [Fact]
    public void AddEavDataProcessors_RegistersBuiltInsOnce()
    {
        var services = new ServiceCollection();
        services.AddEavDataProcessors();

        var registrations = services
            .Where(descriptor => descriptor.ServiceType == typeof(IDataProcessor))
            .ToList();

        Equal(1, registrations.Count(descriptor => descriptor.ImplementationType == typeof(DataProcessor)));
        Equal(1, registrations.Count(descriptor => descriptor.ImplementationType?.FullName == "ToSic.Eav.Metadata.Sys.PermissionDataProcessor"));
    }

    [Fact]
    public void AddEavDataProcessors_MultipleCalls_DoNotDuplicateInterfaceRegistrations()
    {
        var services = new ServiceCollection();

        services.AddEavDataProcessors();
        services.AddEavDataProcessors();

        var duplicateDescriptors = services
            .Where(descriptor => descriptor.ServiceType == typeof(IDataProcessor))
            .GroupBy(descriptor => descriptor.ImplementationType)
            .Where(group => group.Key != null && group.Count() > 1)
            .ToList();

        Empty(duplicateDescriptors);
    }

    [Fact]
    public void AddEavDataProcessors_DiscoversAndResolvesExternalImplementation()
    {
        var services = new ServiceCollection();
        services.AddEavDataProcessors();

        var registrations = services
            .Where(descriptor => descriptor.ServiceType == typeof(IDataProcessor))
            .ToList();

        Contains(registrations, descriptor => descriptor.ImplementationType == typeof(ExternalMockDataProcessor));
        Contains(services, descriptor => descriptor.ServiceType == typeof(ExternalMockDataProcessor));

        using var provider = services.BuildServiceProvider();
        NotNull(provider.GetRequiredService<ExternalMockDataProcessor>());
    }

    private sealed class ExternalMockDataProcessor : DataProcessorBase;
}
