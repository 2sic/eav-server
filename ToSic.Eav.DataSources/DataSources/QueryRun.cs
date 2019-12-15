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
            var query = new Query(ZoneId, AppId, queryDef, Configuration.LookUps, false, Log);

            // add all params
            query.Params(runEntity.GetBestValue<string>(FieldParams));
            //var qParams = query.Definition.GenerateParamsDic(runEntity.GetBestValue<string>(FieldParams));
            //foreach (var qP in qParams)
            //{
            //    Log.Add($"Params:{qP.Key}={qP.Value}");
            //    query.Param(qP.Key, qP.Value);
            //}

            return wrapLog("ok", query.Out);
        }

	}
}