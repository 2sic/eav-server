using Microsoft.Extensions.DependencyInjection;
using ToSic.Eav.Data.Sys.PropertyDump;

namespace ToSic.Eav.Data.Stack.DumpTests.CustomSetup;

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddTransient<IPropertyDumpService, PropertyDumpService>();
        services.AddTransient<IPropertyDumper, ObjectTrivialDumper>();
        services.AddTransient<IPropertyDumper, ObjectMainDumper>();
        services.AddTransient<IPropertyDumper, ObjectChildDumper>();
    }
}
