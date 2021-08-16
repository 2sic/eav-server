using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Data;
using ToSic.Eav.DataSources.Queries;
using ToSic.Eav.Documentation;
using ToSic.Eav.LookUp;

namespace ToSic.Eav.DataSources
{
	/// <summary>
	/// Run another query and provide the resulting data. The settings will provide the params for the inner query.
	/// </summary>
    [PublicApi_Stable_ForUseInYourCode]
	[VisualQuery(
        NiceName = "Query Run",
        UiHint = "Get data from another Query",
        Icon = "launch",
        Type = DataSourceType.Source,
        GlobalName = "ToSic.Eav.DataSources.QueryRun, ToSic.Eav.DataSources",
        DynamicOut = true,
	    ExpectsDataOfType = "78d25ea6-66cc-44a2-b45d-77749cd9420a",
        HelpLink = "https://r.2sxc.org/QueryRun"
        )]

    // ReSharper disable once UnusedMember.Global
    public class QueryRun : DataSourceBase
	{
        #region Configuration-properties
        /// <inheritdoc/>
        [PrivateApi]
	    public override string LogId => "DS.QryRun";

        private const string FieldQuery = "Query";
        private const string FieldParams = "Params";

        /// <summary>
        /// Indicates whether to show drafts or only Published Entities. 
        /// </summary>
        public bool ShowDrafts
        {
            get => bool.Parse(Configuration[QueryConstants.ParamsShowDraftKey]);
            set => Configuration[QueryConstants.ParamsShowDraftKey] = value.ToString();
        }

        #endregion

        /// <summary>
        /// Constructs a new QueryRun
        /// </summary>
        [PrivateApi]
		public QueryRun()
        {
            ConfigMask(QueryConstants.ParamsShowDraftKey, QueryConstants.ParamsShowDraftToken);
        }

        #region Out
        /// <inheritdoc/>
        public override IDictionary<string, IDataStream> Out 
            => _out ?? (_out = new StreamDictionary(this, Query?.Out  ?? new Dictionary<string, IDataStream>(StringComparer.InvariantCultureIgnoreCase)));

        private IDictionary<string, IDataStream> _out;
        #endregion

        #region Surface the inner query in the API in case we need to look into it from our Razor Code

        /// <summary>
        /// The inner query object. Will be initialized the first time it's accessed.
        /// </summary>
        [PrivateApi("not sure if showing this has any value - probably not")]
        public Query Query => _query ?? (_query = BuildQuery());
        private Query _query;
        #endregion

        private Query BuildQuery()
        {
            var wrapLog = Log.Call<Query>();
            // parse config to be sure we get the right query name etc.
            Configuration.Parse();

            #region get the configEntity
            // go through the metadata-source to find it, since it's usually only used in LookUps
            var metadataLookUp = (Configuration.LookUpEngine.FindSource(QueryBuilder.ConfigKeyPartSettings)// .Sources[QueryBuilder.ConfigKeyPartSettings] 
                as LookUpInLookUps)
                ?.Providers.FirstOrDefault(p => p is LookUpInMetadata) as LookUpInMetadata;

            // if found, initialize and get the metadata entity attached
            metadataLookUp?.Initialize();
            var configEntity = metadataLookUp?.Data;
            #endregion

            #region check for various missing configurations / errors

            // prepare empty result, as we may need it in various use cases
            //var emptyResult = new Dictionary<string, IDataStream>(StringComparer.InvariantCultureIgnoreCase);

            // quit if nothing found
            if (configEntity == null)
            {
                Log.Add("no configuration found - empty list");
                return wrapLog("silent error", null /*emptyResult*/);
            }

            Log.Add($"Found query settings'{configEntity.GetBestTitle()}' ({configEntity.EntityId}), will continue");


            var queryDef = configEntity.Children(FieldQuery).FirstOrDefault();
            if (queryDef == null)
            {
                Log.Add("can't find query in configuration - empty list");
                return wrapLog("silent error", null /*emptyResult*/);
            }
            #endregion

            Log.Add($"Found query '{queryDef.GetBestTitle()}' ({queryDef.EntityId}), will continue");

            // create the query & set params
            var query = new Query(DataSourceFactory).Init(ZoneId, AppId, queryDef, LookUpWithoutParams(), ShowDrafts, null, Log);
            query.Params(ResolveParams(configEntity));
            return wrapLog("ok", query);
        }

        /// <summary>
        /// Create a new lookup machine and remove the params which would be in there right now
        /// note that internally, query will generate another params for this
        /// </summary>
        /// <returns></returns>
        private LookUpEngine LookUpWithoutParams()
        {
            var lookUpsWithoutParams = new LookUpEngine(Configuration.LookUpEngine, Log, true);
            if (lookUpsWithoutParams.HasSource(QueryConstants.ParamsLookup))
                lookUpsWithoutParams.Sources.Remove(QueryConstants.ParamsLookup);
            // 1.1 note: can't add Override here because the underlying params don't exist yet - so an override wouldn't keep them
            return lookUpsWithoutParams;
        }

        /// <summary>
        ///  Take the new params and resolve them in the context of this query
        /// </summary>
        /// <param name="runEntity"></param>
        /// <returns></returns>
        private IDictionary<string, string> ResolveParams(IEntity runEntity)
        {
            var fieldParams = runEntity.Value<string>(FieldParams);
            var newParamsDic = QueryDefinition.GenerateParamsDic(fieldParams, Log);
            var resultingParams = Configuration.Parse(newParamsDic);
            Log.Add($"Resolved wrapper params - found {resultingParams.Count} ["
                    + string.Join(",", resultingParams.Select(p => p.Key + "=" + p.Value))
                    + "]");
            return resultingParams;
        }
    }
}