using ToSic.Eav.WebApi.Sys.Dto;

namespace ToSic.Eav.WebApi.Sys.Admin.Query;

public interface IQueryController
{
    // Replaced by DataSource System.DataSources through query System.SysData.
    //IEnumerable<DataSourceDto> DataSources(int zoneId, int appId);

    // Replaced by DataSource System.QueryDefinition through query System.SysData.
    //QueryDefinitionDto Get(int appId, int? id = null);

    void Clone(int appId, int id);

    bool Delete(int appId, int id);

    bool Import(EntityImportDto args);

    QueryRunDto RunDev(int appId, int id, int top = 0);

    QueryDefinitionDto Save(QueryDefinitionDto data, int appId, int id);

    QueryRunDto DebugStream(int appId, int id, string from, string @out, int top = 25);
}