using ToSic.Eav.Apps.Sys.Work;
using ToSic.Eav.DataSource.Query.Sys;
using ToSic.Eav.DataSources.Query.Sys;
using ToSic.Eav.Serialization;

namespace ToSic.Eav.DataSources.ImportExport.Sys;

/// <summary>
/// Import a query
/// </summary>
[ShowApiWhenReleased(ShowApiMode.Never)]
public class WorkQueryImport(AppWorkChain<WorkQueryCopy> workUnitQueryCopy, Generator<IJsonDataReader, IJsonDataReader.Options> jsonSerializer, LazySvc<QueryDefinitionService> queryDefSvc)
    : ServiceWithSetup<IAppWorkContext>("Qry.Imp", connect: [workUnitQueryCopy, jsonSerializer, queryDefSvc])
{
    
    public bool Import(int appId, string json)
    {
        var l = Log.Fn<bool>();
        try
        {
            var workUnit = workUnitQueryCopy.New(MyOptions);
            var deser = jsonSerializer.New(new(MyOptions.AppReader));
            var ents = deser.Deserialize(json);
            var qdef = queryDefSvc.Value.GetDefinition(appId, ents);
            workUnit.SaveCopy(qdef);

            return l.ReturnTrue();
        }
        catch (Exception ex)
        {
            l.Ex(ex);
            throw new($"Couldn't import - {ex?.Message ?? "probably bad file format"}.", ex);
        }
    }

}
