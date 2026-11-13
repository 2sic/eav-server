using ToSic.Eav.DataSources.Query.Dev.Sys;
using ToSic.Eav.ImportExport.Sys;
using ToSic.Eav.ImportExport.Sys.JsonData;

namespace ToSic.Eav.WebApi.Sys.Admin.Query;

public interface IQueryController
{
    void Clone(int appId, int id);

    bool Delete(int appId, int id);

    bool Import(DataFileImportOptions args);

    QueryRunDto RunDev(int appId, int id, int top = 0);

    QueryDefinitionDto Save(QueryDefinitionDto data, int appId, int id);

    QueryRunDto DebugStream(int appId, int id, string from, string @out, int top = 25);
}