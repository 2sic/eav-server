using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Data;
using ToSic.Eav.DataSource;
using ToSic.Eav.DataSource.Query;
using ToSic.Eav.DataSource.Streams;
using ToSic.Eav.DataSource.VisualQuery;
using ToSic.Eav.DataSources.LookUp;
using ToSic.Lib.Logging;
using ToSic.Eav.LookUp;
using ToSic.Lib.DI;
using ToSic.Lib.Documentation;

namespace ToSic.Eav.DataSources
{
	/// <summary>
	/// Run another query and provide the resulting data. The settings will provide the params for the inner query.
	/// </summary>
    [PublicApi_Stable_ForUseInYourCode]
	[VisualQuery(
        NiceName = "Query Run",
        UiHint = "Get data from another Query",
        Icon = Icons.Launch,
        Type = DataSourceType.Source,
        NameId = "ToSic.Eav.DataSources.QueryRun, ToSic.Eav.DataSources",
        DynamicOut = true,
	    ConfigurationType = "78d25ea6-66cc-44a2-b45d-77749cd9420a",
        HelpLink = "https://r.2sxc.org/QueryRun"
        )]

    // ReSharper disable once UnusedMember.Global
    public class QueryRun : Eav.DataSource.DataSourceBase
	{
        private readonly Generator<Query> _queryGenerator;

        #region Configuration-properties

        private const string FieldQuery = "Query";
        private const string FieldParams = "Params";

        ///// <summary>
        ///// Indicates whether to show drafts or only Published Entities. 
        ///// </summary>
        //[PrivateApi("not sure if this should be public, probably not")]
        //[Configuration(Fallback = false)]
        //private bool ShowDrafts => Configuration.GetThis(QueryConstants.ShowDraftsDefault);

        #endregion

        #region Constructor
        

        /// <summary>
        /// Constructs a new QueryRun
        /// </summary>
        [PrivateApi]
		public QueryRun(MyServices services, Generator<Query> queryGenerator) : base(services, $"{DataSourceConstants.LogPrefix}.QryRun")
        {
            ConnectServices(
                _queryGenerator = queryGenerator
            );
        }
        #endregion

        #region Out
        /// <inheritdoc/>
        public override IReadOnlyDictionary<string, IDataStream> Out => (_out ?? (_out = new StreamDictionary(this, Query?.Out))).AsReadOnly();

        private StreamDictionary _out;
        #endregion

        #region Surface the inner query in the API in case we need to look into it from our Razor Code

        /// <summary>
        /// The inner query object. Will be initialized the first time it's accessed.
        /// </summary>
        [PrivateApi("not sure if showing this has any value - probably not")]
        public Query Query => _query ?? (_query = BuildQuery());
        private Query _query;
        #endregion

        private Query BuildQuery() => Log.Func(() =>
        {
            // parse config to be sure we get the right query name etc.
            Configuration.Parse();

            #region get the configEntity

            // go through the metadata-source to find it, since it's usually only used in LookUps
            // the LookUp Root is either a LookUpInQueryMetadata, or a group of lookups which has one inside it
            // It's a LookUpInLookUps if the QueryBuilder overwrote it with SaveDraft infos...
            // which we're not sure yet 2023-03-14 2dm if it's in use anywhere
            var lookUpRoot = Configuration.LookUpEngine.FindSource(DataSourceConstants.MyConfigurationSourceName);
            var metadataLookUp = lookUpRoot as LookUpInQueryMetadata
                                 ?? (lookUpRoot as LookUpInLookUps)
                                 ?.Providers.FirstOrDefault(p => p is LookUpInQueryMetadata) as LookUpInQueryMetadata;

            // if found, initialize and get the metadata entity attached
            metadataLookUp?.Initialize();
            var configEntity = metadataLookUp?.Data;

            #endregion

            #region check for various missing configurations / errors

            // quit if nothing found
            if (configEntity == null)
            {
                Log.A("no configuration found - empty list");
                return (null, "silent error");
            }

            Log.A($"Found query settings'{configEntity.GetBestTitle()}' ({configEntity.EntityId}), will continue");


            var queryDef = configEntity.Children(FieldQuery).FirstOrDefault();
            if (queryDef == null)
            {
                Log.A("can't find query in configuration - empty list");
                return (null, "silent error");
            }

            #endregion

            Log.A($"Found query '{queryDef.GetBestTitle()}' ({queryDef.EntityId}), will continue");

            // create the query & set params
            var query = _queryGenerator.New().Init(ZoneId, AppId, queryDef, LookUpWithoutParams());
            query.Params(ResolveParams(configEntity));
            return (query, "ok");
        });

        /// <summary>
        /// Create a new lookup machine and remove the params which would be in there right now
        /// note that internally, query will generate another params for this
        /// </summary>
        /// <returns></returns>
        private LookUpEngine LookUpWithoutParams()
        {
            var lookUpsWithoutParams = new LookUpEngine(Configuration.LookUpEngine, Log, true);
            if (lookUpsWithoutParams.HasSource(DataSourceConstants.ParamsSourceName))
                lookUpsWithoutParams.Sources.Remove(DataSourceConstants.ParamsSourceName);
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
            Log.A($"Resolved wrapper params - found {resultingParams.Count} ["
                    + string.Join(",", resultingParams.Select(p => p.Key + "=" + p.Value))
                    + "]");
            return resultingParams;
        }
    }
}