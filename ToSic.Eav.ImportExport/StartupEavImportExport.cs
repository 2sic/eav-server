using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ToSic.Eav.ImportExport.Internal;
using ToSic.Eav.ImportExport.Internal.Zip;
using ToSic.Eav.Integration;
using ToSic.Eav.Integration.Environment;
using ToSic.Eav.Internal.Environment;

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.Apps;

[ShowApiWhenReleased(ShowApiMode.Never)]
public static class StartupEavImportExport
{
    public static IServiceCollection AddEavImportExport(this IServiceCollection services)
    {
        services.TryAddTransient<AppFileManager>();

        // More services

        services.TryAddTransient<ImportService>();

        services.TryAddTransient<ZipExport>();
        services.TryAddTransient<ZipImport>();
        services.TryAddTransient<ZipImport.MyServices>();
        services.TryAddTransient<ZipFromUrlImport>();
        
        // export import stuff
        services.TryAddTransient<ExportImportValueConversion>();
        services.TryAddTransient<XmlImportWithFiles>(); // Note: added v19.03.03 2025-03-11 by 2dm https://github.com/2sic/2sxc/issues/3598
        services.TryAddTransient<XmlImportWithFiles.MyServices>();

        return services;
    }

    /// <summary>
    /// This will add Do-Nothing services which will take over if they are not provided by the main system
    /// In general this will result in some features missing, which many platforms don't need or care about
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    /// <remarks>
    /// All calls in here MUST use TryAddTransient, and never without the Try
    /// </remarks>
    public static IServiceCollection AddEavImportExportFallback(this IServiceCollection services)
    {
        services.TryAddTransient<IEnvironmentLogger, EnvironmentLoggerUnknown>();
        services.TryAddTransient<XmlExporter, XmlExporterUnknown>();
        services.TryAddTransient<IImportExportEnvironment, ImportExportEnvironmentUnknown>();

        return services;
    }

}