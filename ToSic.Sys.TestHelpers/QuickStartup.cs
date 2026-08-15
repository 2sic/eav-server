using Microsoft.Extensions.DependencyInjection;

namespace ToSic.Sys;

/// <summary>
/// Quick helper to create a startup class with a single action to configure services.
/// This is mostly used for startup classes underneath a test class.
/// </summary>
/// <param name="configureServices">The action to configure services.</param>
public abstract class QuickStartup(Action<IServiceCollection> configureServices)
{
    public virtual void ConfigureServices(IServiceCollection services) =>
        configureServices(services);

}
