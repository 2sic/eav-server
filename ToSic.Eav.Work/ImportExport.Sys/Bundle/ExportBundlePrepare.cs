using ToSic.Eav.Models;
using ToSic.Eav.Serialization;

namespace ToSic.Eav.ImportExport.Sys.Bundle;

[ShowApiWhenReleased(ShowApiMode.Never)]
public class ExportBundlePrepare(Generator<IJsonDataWriter> jsonSerializer)
    : ServiceWithSetup<IAppWorkContext>("Api.EaCtEx", connect: [jsonSerializer])
{
    
    internal (ExportConfiguration export, string fileContent) CreateBundleExport(Guid exportConfiguration, int indentation)
    {
        var l = Log.Fn<(ExportConfiguration export, string fileContent)>($"create bundle export for ExportConfiguration:{exportConfiguration}");

        var export = ExportConfigurationBuildOrThrow(exportConfiguration);

        // find all decorator metadata of type SystemExportDecorator
        l.A($"metadataExportMarkers:{export.ExportMarkers.Count}");

        var serializer = jsonSerializer.New();

        var bundleBuilder = new JsonBundleBuilder(MyOptions.AppReader, Log);
        var bundle = bundleBuilder.BundleBuild(export, serializer);

        // create a file which contains this new bundle
        var fileContent = serializer.SerializeJsonBundle(bundle, indentation);

        // give it to the browser with the name specified in the Export Configuration
        l.A($"OK, export fileName:{export.FileName}, size:{fileContent.Length}");

        return l.ReturnAsOk((export, fileContent));
    }

    private ExportConfiguration ExportConfigurationBuildOrThrow(Guid exportConfigGuid)
    {
        var l = Log.Fn<ExportConfiguration>($"build ExportConfiguration:{exportConfigGuid}");
        var systemExportConfiguration = MyOptions.AppReader.List.GetModel<ExportConfiguration>(exportConfigGuid,
            options: new()
            {
                NullHandling = NullHandling.ReturnNull,
            }
            /*nullHandling: ModelNullHandling.PreferNull*/);
        return systemExportConfiguration is not null
            ? l.ReturnAsOk(systemExportConfiguration)
            : throw l.Ex(new KeyNotFoundException($"ExportConfiguration:{exportConfigGuid} is missing"));
    }

}
