using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ToSic.Eav.ImportExport.Json;
using ToSic.Eav.Metadata;
using ToSic.Eav.Serialization;

namespace ToSic.Eav.ImportExport
{
    public static class ImportExportStartup
    {
        public static IServiceCollection AddImportExport(this IServiceCollection services)
        {
            services.TryAddTransient<IDataSerializer, JsonSerializer>();
            services.TryAddTransient<IDataDeserializer, JsonSerializer>();

            return services;
        }

    }
}
