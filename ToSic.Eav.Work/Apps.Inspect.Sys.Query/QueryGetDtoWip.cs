using ToSic.Eav.Data.Sys.Entities;
using ToSic.Eav.DataFormats.EavLight.Sys;
using ToSic.Eav.DataSource.Query.Sys;
using ToSic.Eav.Metadata;

namespace ToSic.Eav.Apps.Inspect.Sys.Query;

/// <inheritdoc />
/// <summary>
/// Web API Controller for the Pipeline Designer UI.
///
/// It's just a base controller, because some methods need to be added at the SXC level which don't exist in the EAV.
/// </summary>
[ShowApiWhenReleased(ShowApiMode.Never)]
public class QueryGetDtoWip(Generator<IAppReaderFactory> appStates, LazySvc<QueryDefinitionService> queryDefSvc, LazySvc<ConvertEntityToEavLight> entToDicLazy)
    : ServiceBase("Eav.QrGtDto", connect: [appStates, queryDefSvc, entToDicLazy])
{

    /// <summary>
    /// Get a Pipeline with DataSources
    /// </summary>
    public QueryDefinitionDto Get(int appId, int? id = null)
    {
        var l = Log.Fn<QueryDefinitionDto>($"a#{appId}, id:{id}");

        if (!id.HasValue)
            return l.Return(new(), "no id, empty");

        var appState = appStates.New().Get(appId);
        var qDef = queryDefSvc.Value.GetDefinition(appState, id.Value);

        #region Deserialize some Entity-Values

        var pipeline = (qDef as ICanBeEntity).Entity.AsDictionary();
        pipeline[nameof(QueryDefinition.StreamWiring)] = qDef.Connections;

        var converter = entToDicLazy.Value;
        converter.Type.Serialize = true;
        converter.Type.WithDescription = true;
        converter.WithGuid = true;

        var dataSources = qDef.Parts
            .Select(part =>
            {
                var metadata = appState.Metadata.GetMetadata(TargetTypes.Entity, part.Guid);
                var partDto = part.AsDictionary();
                partDto.Add("Metadata", converter.Convert(metadata));
                return partDto;
            })
            .ToListOpt();

        var queryDto = new QueryDefinitionDto
        {
            Pipeline = pipeline!,
            DataSources = dataSources!,
        };


        #endregion

        return l.ReturnAsOk(queryDto);
    }
}