using Microsoft.Extensions.DependencyInjection;
using ToSic.Eav.Internal.Environment;
using ToSic.Eav.Repository.Efc.Tests.Mocks;

#pragma warning disable CA1822

namespace ToSic.Eav.Repository.Efc.Tests.ImportApps;

// NOTE: QUITE A FEW DUPLICATES OF THIS - may want to consolidate

/// <summary>
/// A Startup helper for tests which need Dependency-Injection setup for EAV Core.
/// </summary>
/// <remarks>
/// Use by adding this kind of attribute to your test class:
/// `[Startup(typeof(TestStartupEavCore))]`
/// </remarks>
public class Startup: StartupTestsApps
{
    /// <summary>
    /// Startup helper
    /// </summary>
    public override void ConfigureServices(IServiceCollection services) =>
        base.ConfigureServices(
            services
                // not sure if this is needed or used here...probably not
                .AddTransient<IImportExportEnvironment, ImportExportEnvironmentMock>()
        );
}