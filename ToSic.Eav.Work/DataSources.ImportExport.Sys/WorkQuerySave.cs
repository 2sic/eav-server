using System.Text.Json;
using ToSic.Eav.Apps.Sys.Work;
using ToSic.Eav.DataSource.Query.Sys;
using ToSic.Eav.DataSources.Query.Dev.Sys;
using ToSic.Eav.DataSources.Query.Sys;
using ToSic.Eav.Serialization.Sys.Json;
using QueryDefinition = ToSic.Eav.DataSource.Query.Sys.QueryDefinition;

namespace ToSic.Eav.DataSources.ImportExport.Sys;

/// <summary>
/// Save a query, typically from the UI.
/// </summary>
[ShowApiWhenReleased(ShowApiMode.Never)]
public class WorkQuerySave(AppWorkChain<WorkQueryMod> workUnitQueryMod, LazySvc<QueryGetDtoWip> queryGetDtoWipLazy)
    : ServiceWithSetup<IAppWorkContext>("Qry.Save", connect: [workUnitQueryMod, queryGetDtoWipLazy])
{
    
    /// <summary>
    /// Save Pipeline
    /// </summary>
    /// <param name="data">JSON object { pipeline: pipeline, dataSources: dataSources }</param>
    /// <param name="appId">AppId this Pipeline belongs to</param>
    /// <param name="id">PipelineEntityId</param>
    public QueryDefinitionDto Save(QueryDefinitionDto data, int id)
    {
        var l = Log.Fn<QueryDefinitionDto>($"save pipe: a#{MyOptions.AppId}, id#{id}");
        // assemble list of all new data-source guids, for later re-mapping when saving
        var newDsGuids = data.DataSources
            .Where(d => d.ContainsKey("EntityGuid"))
            .Select(d => d["EntityGuid"].ToString()!)
            .Where(g => g != "Out" && !g.StartsWith("unsaved"))
            .Select(Guid.Parse)
            .ToListOpt();

        // Update Pipeline Entity with new Wirings etc.
        var wiringString = data.Pipeline[nameof(QueryDefinition.StreamWiring)]?.ToString() ?? "";
        var wirings =
            JsonSerializer.Deserialize<List<QueryWire>>(wiringString, JsonOptions.UnsafeJsonWithoutEncodingHtml)
            ?? [];

        workUnitQueryMod.New(MyOptions).Update(id, data.DataSources, newDsGuids, data.Pipeline, wirings);

        return l.ReturnAsOk(queryGetDtoWipLazy.Value.Get(MyOptions.AppId, id));
    }

}
