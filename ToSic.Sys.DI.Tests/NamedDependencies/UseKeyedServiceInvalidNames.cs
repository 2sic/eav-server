using Microsoft.Extensions.DependencyInjection;

namespace ToSic.NamedDependencies;


#region Invalid Names

/// <summary>
/// This will throw an error during test setup, so we can't run it.
/// </summary>
/// <param name="serviceProvider"></param>
public class UseKeyedServiceInvalidName(IServiceProvider serviceProvider)
{
    /// <summary>
    /// Faulty service which would request another service with an invalid name.
    /// </summary>
    /// <param name="services"></param>
    private class WouldNeedInvalidName([FromKeyedServices("InvalidName")] IMockNamedService services);

    /// <summary>
    /// Add the faulty service to the DI container, so we can test if it throws an error when requested.
    /// </summary>
    public class Startup: ToSic.NamedDependencies.Startup
    {
        public override void ConfigureServices(IServiceCollection services)
            => base.ConfigureServices(services.AddTransient<WouldNeedInvalidName>());
    }
    
    /// <summary>
    /// Verify that it will throw.
    /// </summary>
    [Fact]
    public void RequestingServiceThrows()
        => Throws<InvalidOperationException>(serviceProvider.GetServices<WouldNeedInvalidName>);
}

public class UseKeyedServiceListInvalidName([FromKeyedServices("InvalidName")] IEnumerable<IMockNamedService> services)
{
    [Fact]
    public void IsEmpty()
        => Empty(services);
}

#endregion