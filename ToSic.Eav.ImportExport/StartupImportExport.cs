using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ToSic.Eav.ImportExport.Json;
using ToSic.Eav.Persistence.File;
using ToSic.Eav.Persistence.Xml;
using ToSic.Eav.Repositories;
using ToSic.Eav.Serialization;

namespace ToSic.Eav.ImportExport
{
    public static class StartupImportExport
    {
        public static IServiceCollection AddImportExport(this IServiceCollection services)
        {
            services.TryAddTransient<IDataSerializer, JsonSerializer>();
            services.TryAddTransient<IDataDeserializer, JsonSerializer>();

            services.TryAddTransient<JsonSerializer>();

            services.TryAddTransient<XmlSerializer>();

            services.TryAddTransient<FileSystemLoader>();

            // WIP 12.10+ with Global Types in a normal AppState
            services.TryAddTransient<PresetAppStateLoader>();
            services.TryAddTransient<IPresetLoader, PresetAppStateLoader>();

            return services;
        }

    }
}
