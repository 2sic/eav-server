using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Apps;
using ToSic.Eav.Apps.DataSources.Types;
using ToSic.Eav.DataSources.VisualQuery;
using ToSic.Eav.Interfaces;

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.DataSources.System
{
    /// <inheritdoc />
    /// <summary>
    /// A DataSource that returns the attributes of a content-type
    /// </summary>
    [VisualQuery(Type = DataSourceType.Source,
        Difficulty = DifficultyBeta.Advanced,
        DynamicOut = false,
        EnableConfig = true,
        ExpectsDataOfType = "37b25044-29bb-4c78-85e4-7b89f0abaa2c",
        HelpLink = "https://github.com/2sic/2sxc/wiki/DotNet-DataSource-ContentTypes")]

    public sealed class ContentTypes: BaseDataSource
	{
        #region Configuration-properties (no config)
	    public override string LogId => "DS.EavCTs";

        private const string AppIdKey = "AppId";
        private const string AppIdField = "AppId";
        private const string ScopeKey = "Scope";
        private const string ScopeField = "Scope";
	    //private const string TryToUseInStream = "not-configured-try-in"; // can't be blank, otherwise tokens fail
	    private const string ContentTypeTypeName = "EAV_ContentTypes";
	    
        // 2dm: this is for a later feature...
	    // ReSharper disable once UnusedMember.Local
        private const string ContentTypeCtGuid = "11001010-251c-eafe-2792-000000000003"; // must check before using


        /// <summary>
        /// The app id
        /// </summary>
        public int DesiredAppId
        {
            get => int.TryParse(Configuration[AppIdKey], out int aid) ? aid : AppId;
            set => Configuration[AppIdKey] = value.ToString();
        }

	    /// <summary>
	    /// The content-type name
	    /// </summary>
	    public string ScopeName
	    {
	        get => Configuration[ScopeKey];
	        set => Configuration[ScopeKey] = value;
	    }

        #endregion

        /// <inheritdoc />
        /// <summary>
        /// Constructs a new Attributes DS
        /// </summary>
        public ContentTypes()
		{
			Out.Add(Constants.DefaultStreamName, new DataStream(this, Constants.DefaultStreamName, GetList));
			//Out.Add("Scopes", new DataStream(this, Constants.DefaultStreamName, GetList));
            Configuration.Add(AppIdKey, $"[Settings:{AppIdField}]");
            Configuration.Add(ScopeKey, $"[Settings:{ScopeField}||Default]");

            CacheRelevantConfigurations = new[] {AppIdKey};
		}

	    private IEnumerable<IEntity> GetList()
	    {
            EnsureConfigurationIsLoaded();

	        var appId = DesiredAppId;

            var read = new AppRuntime(appId, Log);
	        var scp = ScopeName;
	        if (scp == "Default")
	            scp = "2SexyContent";

	        var types = read.ContentTypes.FromScope(scp);

	        var list = types.OrderBy(t => t.Name).Select(t =>
	        {
	            Guid? guid = null;
	            try
	            {
	                if (Guid.TryParse(t.StaticName, out Guid g)) guid = g;
	            }
	            catch
	            {
	                /* ignore */
	            }

	            return new Data.Entity(DesiredAppId, t.ContentTypeId, ContentTypeTypeName, BuildDictionary(t), ContentTypeType.Name.ToString(), entityGuid: guid);
	        });

	        return list;
	    }

	    private static Dictionary<string, object> BuildDictionary(IContentType t) => new Dictionary<string, object>
	    {
	        {ContentTypeType.Name.ToString(), t.Name},
	        {ContentTypeType.StaticName.ToString(), t.StaticName},
	        {ContentTypeType.Description.ToString(), t.Description},
	        {ContentTypeType.IsDynamic.ToString(), t.IsDynamic},

	        {ContentTypeType.Scope.ToString(), t.Scope},
	        {ContentTypeType.AttributesCount.ToString(), t.Attributes.Count},

	        {ContentTypeType.RepositoryType.ToString(), t.RepositoryType.ToString()},
	        {ContentTypeType.RepositoryAddress.ToString(), t.RepositoryAddress},
	    };
	}
}