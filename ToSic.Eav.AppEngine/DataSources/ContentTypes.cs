using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Apps;
using ToSic.Eav.Apps.DataSources.Types;
using ToSic.Eav.Data;
using ToSic.Eav.DataSources.Queries;
using ToSic.Eav.Documentation;
using IEntity = ToSic.Eav.Data.IEntity;

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.DataSources
{
    /// <inheritdoc />
    /// <summary>
    /// A DataSource that all content-types of an app.
    /// </summary>
    [VisualQuery(
        GlobalName = "ToSic.Eav.DataSources.ContentTypes, ToSic.Eav.Apps",
        Type = DataSourceType.Source,
        Difficulty = DifficultyBeta.Advanced,
        DynamicOut = false,
        ExpectsDataOfType = "37b25044-29bb-4c78-85e4-7b89f0abaa2c",
        PreviousNames = new []
            {
                "ToSic.Eav.DataSources.System.ContentTypes, ToSic.Eav.Apps"
            },
        HelpLink = "https://github.com/2sic/2sxc/wiki/DotNet-DataSource-ContentTypes")]
    [PrivateApi("probably should be in own SysInfo folder or something")]
    public sealed class ContentTypes: DataSourceBase
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
        public int OfAppId
        {
            get => int.TryParse(Configuration[AppIdKey], out int aid) ? aid : AppId;
            set => Configuration[AppIdKey] = value.ToString();
        }

	    /// <summary>
	    /// The content-type name
	    /// </summary>
	    public string OfScope
	    {
	        get => Configuration[ScopeKey];
	        set => Configuration[ScopeKey] = value;
	    }

        #endregion

        /// <inheritdoc />
        /// <summary>
        /// Constructs a new ContentTypes DS
        /// </summary>
        [PrivateApi]
        public ContentTypes()
		{
			Provide(GetList);
		    ConfigMask(AppIdKey, $"[Settings:{AppIdField}]");
		    ConfigMask(ScopeKey, $"[Settings:{ScopeField}||Default]");
		}

	    private IEnumerable<IEntity> GetList()
	    {
            ConfigurationParse();

	        var appId = OfAppId;

            var read = new AppRuntime(appId, Log);
	        var scp = OfScope;
	        if (string.IsNullOrWhiteSpace(scp) || string.Equals(scp, "Default", StringComparison.InvariantCultureIgnoreCase))
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

                return Build.Entity(BuildDictionary(t),
                    appId:OfAppId, 
                    id:t.ContentTypeId, 
                    titleField: ContentTypeType.Name.ToString(),
                    typeName: ContentTypeTypeName,
                    guid: guid);

	            //return AsEntity(BuildDictionary(t), ContentTypeType.Name.ToString(), ContentTypeTypeName, t.ContentTypeId, guid, appId: OfAppId);// new Data.Entity(DesiredAppId, t.ContentTypeId, ContentTypeTypeName, BuildDictionary(t), ContentTypeType.Name.ToString(), entityGuid: guid);
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