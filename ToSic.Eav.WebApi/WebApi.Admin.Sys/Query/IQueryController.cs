using ToSic.Eav.DataSources.Query.Dev.Sys;
using ToSic.Eav.ImportExport.Sys;

namespace ToSic.Eav.WebApi.Admin.Sys;

public interface IQueryController: IAppAdminWebApiController
{
    // UI test: App administration > Queries > clone a query.
    void Clone(int appId, int id);

    // UI test: App administration > Queries > delete a query.
    bool Delete(int appId, int id);

    // UI test: App administration > Queries > Import Query.
    bool Import(DataFileImportOptions args);

    // UI test: Open a query in Visual Query and run / test the query.
    QueryRunDto RunDev(int appId, int id, int top = 0);

    // UI test: Open a query in Visual Query, change the diagram, and save.
    QueryDefinitionDto Save(QueryDefinitionDto data, int appId, int id);

    // UI test: Visual Query results > select a data-source stream to debug it.
    QueryRunDto DebugStream(int appId, int id, string from, string @out, int top = 25);
}
