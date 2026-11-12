using ToSic.Eav.Apps.Sys.LogSettings;
using ToSic.Eav.Serialization;

namespace ToSic.Eav.ImportExport.Sys.JsonData;

/// <inheritdoc />
[ShowApiWhenReleased(ShowApiMode.Never)]
public class ImportDataJson(
    Generator<IJsonDataReader, IJsonDataReader.Options> jsonReaderLazy, AppWorkChain<WorkEntitySave> workEntSave, DataImportLogSettings importLogSettings)
    : ServiceWithSetup<IAppWorkContext>("Api.EaCtIm", connect: [workEntSave, jsonReaderLazy, importLogSettings])
{

    #region Detailed Logging

    [field: AllowNull, MaybeNull]
    private LogSettings LogSettings => field ??= importLogSettings.GetLogSettings();

    #endregion



    public bool Import(EntityImportDto args)
    {
        var l = Log.Fn<bool>(message: "import json item" + args.DebugInfo);
        try
        {
            var deserializer = jsonReaderLazy.New(new(MyOptions.AppReader)
            {
                PreferLocalAppTypes = true,
                LogSettings = LogSettings,
            });

            var listToImport = new List<IEntity> { deserializer.Deserialize(args.GetContentString()) };

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