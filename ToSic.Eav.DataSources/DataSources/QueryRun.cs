using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.DataSources.Queries;
using ToSic.Eav.Documentation;
using ToSic.Eav.LookUp;

namespace ToSic.Eav.DataSources
{
	/// <summary>
	/// Run another query and provide the resulting data. The settings will provide the params for the query.
	/// </summary>
    [PublicApi]
	[VisualQuery(GlobalName = "ToSic.Eav.DataSources.QueryRun, ToSic.Eav.DataSources",
        Type = DataSourceType.Source,
        NiceName = "Query Run",
        Icon = "filter",
        DynamicOut = true,
	    ExpectsDataOfType = "78d25ea6-66cc-44a2-b45d-77749cd9420a",
        HelpLink = "https://docs.2sxc.org/api/dot-net/ToSic.Eav.DataSources.QueryRun.html"
        )]

    public class QueryRun : DataSourceBase
	{
        #region Configuration-properties
        /// <inheritdoc/>
        [PrivateApi]
	    public override string LogId => "DS.QryRun";

        private const string FieldQuery = "Query";
        private const string FieldParams = "Params";

		#endregion

		/// <summary>
		/// Constructs a new QueryRun
		/// </summary>
		[PrivateApi]
		public QueryRun()
		{
            OutIsDynamic = true;
        }

        #region Out
        /// <inheritdoc/>
        public override IDictionary<string, IDataStream> Out => _out ?? (_out = CreateOutWithAllStreams());

        private IDictionary<string, IDataStream> _out;
        #endregion

        private IDictionary<string, IDataStream> CreateOutWithAllStreams()
        {
            var wrapLog = Log.Call<IDictionary<string, IDataStream>>();
            Configuration.Parse();

            var emptyResult = new Dictionary<string, IDataStream>(StringComparer.OrdinalIgnoreCase);

            // ensure we have a configuration
            var metadataLookUp = !(Configuration.LookUps.Sources[QueryBuilder.ConfigKeyPartSettings] 
                is LookUpInLookUps settingsLookUp)
                ? null
                : settingsLookUp.Providers.FirstOrDefault(p => p is LookUpInMetadata) as LookUpInMetadata;

            // if found, initialize and get the metadata entity attached
            metadataLookUp?.Initialize();
            var runEntity = metadataLookUp?.Data;

            // quit if nothing found
            if(runEntity == null)
            {
                Log.Add("no configuration found - empty list");
                return wrapLog("silent error", emptyResult);
            }

            Log.Add($"Found query settings'{runEntity.GetBestTitle()}' ({runEntity.EntityId}), will continue");


            var queryDef = runEntity.Children(FieldQuery).FirstOrDefault();
            if (queryDef == null)
            {
                Log.Add("can't find query in configuration - empty list");
                return wrapLog("silent error", emptyResult);
            }
            Log.Add($"Found query '{queryDef.GetBestTitle()}' ({queryDef.EntityId}), will continue");

            // Note: ShowDrafts is false - but actually it will work
            // because that would only create an additional data-source for drafts-info
            // which was already created previously when the ConfigurationProvider for this DS was made
            
            // 0. Take the new params and resolve them in the context of this query
            // var tempLookUpForParams = new LookUpEngine(Configuration.LookUps, Log);
            var fieldParams = runEntity.GetBestValue<string>(FieldParams);
            var newParamsDic = QueryDefinition.GenerateParamsDic(fieldParams, Log);
            var resultingParams = Configuration.Parse(newParamsDic);
            Log.Add($"Resolved wrapper params - found {resultingParams.Count} [" 
                    + string.Join(",", resultingParams.Select(p => p.Key + "=" + p.Value)) 
                    + "]");

            // 1. Create a new lookup machine and remove the params which would be in there right now
            // note that internally, query will generate another params for this
            var lookUpsWithoutParams = new LookUpEngine(Configuration.LookUps, Log);
            if (lookUpsWithoutParams.Sources.ContainsKey(QueryConstants.ParamsLookup))
                lookUpsWithoutParams.Sources.Remove(QueryConstants.ParamsLookup);
            // note: can't add Override, because the underlying params don't exist yet - so an override wouldn't keep them
            // lookUpsWithoutParams.AddOverride(new LookUpInDictionary(QueryConstants.ParamsLookup, resultingParams));

            // 2. create the query
            var query = new Query(ZoneId, AppId, queryDef, lookUpsWithoutParams, false, this, Log);

            #region 3. Set Params
            // #3a access the params to be sure they were loaded
            //var oldParams = query.Params();
            query.Params(resultingParams);

            // #3b add all params
            //var wrapLogParams = Log.Call("will override params");
            //query.Params(fieldParams);
            //wrapLogParams("done");

            #endregion

            return wrapLog("ok", query.Out);
        }

	}
}