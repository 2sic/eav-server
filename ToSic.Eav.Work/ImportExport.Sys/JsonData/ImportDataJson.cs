using ToSic.Eav.Apps.Sys.LogSettings;
using ToSic.Eav.Serialization;
using ToSic.Sys.Utils;

namespace ToSic.Eav.ImportExport.Sys.JsonData;

/// <inheritdoc />
[ShowApiWhenReleased(ShowApiMode.Never)]
public class ImportDataJson(
    Generator<IJsonDataReader, IJsonDataReader.Options> jsonReaderLazy, AppWorkChain<WorkEntitySave> workEntSave, DataImportLogSettings importLogSettings)
    : ServiceWithSetup<IAppWorkContext>("Api.EaCtIm", connect: [workEntSave, jsonReaderLazy, importLogSettings])
{

    public bool Import(DataFileImportOptions args)
    {
        var l = Log.Fn<bool>(message: "import json item" + args);
        try
        {
            var deserializer = jsonReaderLazy.New(new(MyOptions.AppReader)
            {
                PreferLocalAppTypes = true,
                LogSettings = importLogSettings.GetLogSettings(),
            });

            var listToImport = new List<IEntity> { deserializer.Deserialize(args.ContentBase64.Base64ToUtf8()) };

            workEntSave.New(MyOptions).Import(listToImport);

            return l.ReturnTrue();
        }
        catch (ArgumentException)
        {
            throw;
        }
        catch (Exception ex)
        {
            l.Ex(ex);
            throw new("Couldn't import - probably bad file format", ex);
        }
    }
}