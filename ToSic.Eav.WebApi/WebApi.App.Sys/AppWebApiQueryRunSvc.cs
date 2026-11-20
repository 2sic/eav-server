using System.Net;
using ToSic.Eav.Apps.Sys.Permissions;
using ToSic.Eav.Context;
using ToSic.Eav.DataFormats.EavLight;
using ToSic.Eav.DataSource;
using ToSic.Eav.DataSource.Query.Sys;
using ToSic.Eav.DataSource.Sys.Convert;
using ToSic.Eav.LookUp.Sys.Engines;
using ToSic.Eav.WebApi.OData.Sys;
using ToSic.Eav.WebApi.Sys.Helpers.Http;
using ToSic.Sys.OData;
using ToSic.Sys.Security.Permissions;

namespace ToSic.Eav.WebApi.App.Sys;

/// <summary>
/// In charge of running and converting Queries on the fly
/// They will only be delivered if the security is confirmed - it must be publicly available
/// </summary>
[ShowApiWhenReleased(ShowApiMode.Never)]
public class AppWebApiQueryRunSvc(AppQueryODataHelper oDataHelper, Generator<IConvertToEavLight> converter, PermissionCheckFactory permChecks, QueryBuilder<Query> qryBuilder)
    : ServiceBase("Sxc.ApiApQ", connect: [oDataHelper, converter, permChecks, qryBuilder])
{
    public const string LogSuffix = "AppQry";

    public record AppCtxAndLookup(IContextOfApp AppCtx, ILookUpEngine Lookups);

    /// <summary>
    /// Run Queries which are in a container context
    /// </summary>
    /// <param name="appInfo"></param>
    /// <param name="queryName">query name</param>
    /// <param name="more">additional url parameters for odata</param>
    /// <param name="stream">optional stream name(s)</param>
    /// <param name="includeGuid">Include the guid in the result</param>
    /// <returns></returns>
    public IDictionary<string, IEnumerable<EavLightEntity>> RunQuery(AppCtxAndLookup appInfo, string queryName, QueryParametersDtoFromClient? more, string? stream = null, bool? includeGuid = false)
    {
        var l = Log.Fn<IDictionary<string, IEnumerable<EavLightEntity>>>($"'{queryName}', stream: '{stream}', inclGuid: {includeGuid}");

        if (string.IsNullOrEmpty(queryName))
            throw l.Ex(new ArgumentException($"Missing Parameter: {nameof(queryName)}"));

        var query = GetQueryAndDoPermissionChecks(queryName, appInfo);

        var streams = stream == DataSourceConstants.AllStreams ? null : stream;
        var streamNames = DataSourceConvertHelper.GetBestStreamNames(query, streams);
        var parameters = new Parameters(
            //Context: context,
            AppReader: appInfo.AppCtx.AppReaderRequired,
            Lookups: appInfo.Lookups,
            Query: query,
            QueryName: queryName,
            Streams: streams,
            StreamNames: streamNames,
            IncludeGuids: includeGuid ?? false,
            OData: more?.Guids
        );

        var result = BuildQueryAndRun(parameters);
        return l.Return(result);
    }

    public record Parameters(
        //IContextOfApp Context,
        IAppReader AppReader,
        ILookUpEngine Lookups,
        Query Query,
        string QueryName,
        string? Streams,
        string[] StreamNames,
        bool IncludeGuids,
        string[]? OData
    );


    private Query GetQueryAndDoPermissionChecks(string queryName, AppCtxAndLookup appInfo)
    {
        var app = appInfo.AppCtx.AppReaderRequired;
        var l = Log.Fn<Query?>($"name:{queryName}, app:{app.AppId}");
        var query = qryBuilder.TryBuild(app, queryName, appInfo.Lookups, recurseParents: 3);

        if (query == null)
        {
            var msg = $"query '{queryName}' not found";
            throw l.Done(new KeyNotFoundException(msg));
        }

        l.A($"Check permission on query {query.Definition.Id}");
        var permissionChecker = permChecks.ForItem(appInfo.AppCtx, app, (query.Definition as ICanBeEntity).Entity);
        var readExplicitlyAllowed = permissionChecker.UserMay(GrantSets.ReadSomething).Allowed;

        // Only return query if permissions ok
        if (!(readExplicitlyAllowed || appInfo.AppCtx.User.IsContentAdmin))
        {
            var msg = $"Request not allowed. User does not have read permissions for query '{queryName}'";
            throw l.Done(new PermissionDeniedException("Request not allowed; " + msg));
        }

        return l.Return(query);
    }


    public IDictionary<string, IEnumerable<EavLightEntity>> BuildQueryAndRun(Parameters parameters)
    {
        var (/*context, #DropWithEdit */ _, lookups, query, _, streams, streamNames, includeGuid, guidList) = parameters;

        var l = Log.Fn<IDictionary<string, IEnumerable<EavLightEntity>>>();


        // Pass the originally requested stream so QueryODataParams can map bare OData options
        // to that stream when exactly one stream was explicitly selected.
        var streamOptions = QueryODataParams.CreateMany(d => lookups.LookUp(d), streamNames, streams);

        // v21 support OData filtering, sorting...
        var mustUseOData = streamOptions.Any(so => !so.Value.IsEmptyExceptForSelect());
        if (mustUseOData)
        {
            var oDataResult = oDataHelper.ApplyOData(query, streamOptions, guidList, includeGuid/*, context.Permissions.IsContentAdmin #DropWithEdit*/);
            return l.Return(oDataResult, "processed with OData");
        }

        // New v17 experimental with special fields
        var systemQueryOptions = QueryODataParams.Create(d => lookups.LookUp(d));

        // Classic, lightweight conversion
        var selectFields = streamOptions.ToDictionary(
            pair => pair.Key,
            ICollection<string> (pair) => pair.Value.Select.ToListOpt(),
            StringComparer.OrdinalIgnoreCase
        );
        var dc = converter.PrepareDataConverter(systemQueryOptions, includeGuid/*, context.Permissions.IsContentAdmin #DropWithEdit*/);
        var result = dc.Convert(query, streamNames, guidList, selectFields);
        return l.Return(result, "classic convert");
    }
}
