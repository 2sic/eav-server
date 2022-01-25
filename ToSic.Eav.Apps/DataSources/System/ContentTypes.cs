using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using ToSic.Eav.Apps;
using ToSic.Eav.Apps.DataSources.Types;
using ToSic.Eav.Data;
using ToSic.Eav.DataSources.Queries;
using ToSic.Eav.Documentation;
using IEntity = ToSic.Eav.Data.IEntity;

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.DataSources.System
{
    /// <inheritdoc />
    /// <summary>
    /// A DataSource that all content-types of an app.
    /// </summary>
    [InternalApi_DoNotUse_MayChangeWithoutNotice]
    [VisualQuery(
        NiceName = "Content Types",
        UiHint = "Types of an App",
        Icon = "dns",
        Type = DataSourceType.System,
        GlobalName = "ToSic.Eav.DataSources.System.ContentTypes, ToSic.Eav.Apps",
        Difficulty = DifficultyBeta.Advanced,
        DynamicOut = false,
        ExpectsDataOfType = "37b25044-29bb-4c78-85e4-7b89f0abaa2c",
        PreviousNames = new []
            {
                "ToSic.Eav.DataSources.System.ContentTypes, ToSic.Eav.Apps",
                // not sure if this was ever used...just added it for safety for now
                // can probably remove again, if we see that all system queries use the correct name
                "ToSic.Eav.DataSources.ContentTypes, ToSic.Eav.Apps",
            },
        HelpLink = "https://github.com/2sic/2sxc/wiki/DotNet-DataSource-ContentTypes")]
    // ReSharper disable once UnusedMember.Global
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
	    

        /// <summary>
        /// The app id
        /// </summary>
        public int OfAppId
        {
            get => int.TryParse(Configuration[AppIdKey], out int aid) ? aid : AppId;
            // ReSharper disable once UnusedMember.Global
            set => Configuration[AppIdKey] = value.ToString();
        }

	    /// <summary>
	    /// The content-type name
	    /// </summary>
	    public string OfScope
	    {
	        get => Configuration[ScopeKey];
            // ReSharper disable once UnusedMember.Global
            set => Configuration[ScopeKey] = value;
	    }

        #endregion

        /// <inheritdoc />
        /// <summary>
        /// Constructs a new ContentTypes DS
        /// </summary>
        [PrivateApi]
        public ContentTypes(IAppStates appStates)
		{
            _appStates = appStates;
            Provide(GetList);
		    ConfigMask(AppIdKey, $"[Settings:{AppIdField}]");
		    ConfigMask(ScopeKey, $"[Settings:{ScopeField}||Default]");
		}
        private readonly IAppStates _appStates;

	    private ImmutableArray<IEntity> GetList()
	    {
            var wrapLog = Log.Call<ImmutableArray<IEntity>>();

            Configuration.Parse();

            var appId = OfAppId;

	        var scp = OfScope;
            if (string.IsNullOrWhiteSpace(scp)) scp = Scopes.Default;

            var types = _appStates.Get(appId).ContentTypes.OfScope(scp);
            
            var builder = DataBuilder;
	        var list = types.OrderBy(t => t.Name).Select(t =>
	        {
	            Guid? guid = null;
	            try
	            {
	                if (Guid.TryParse(t.NameId, out Guid g)) guid = g;
	            }
	            catch
	            {
	                /* ignore */
	            }

                return builder.Entity(BuildDictionary(t),
                    appId:OfAppId, 
                    id:t.Id, 
                    titleField: ContentTypeType.Name.ToString(),
                    typeName: ContentTypeTypeName,
                    guid: guid);
	        });

	        var result = list.ToImmutableArray();
            return wrapLog($"{result.Length}", result);
        }

	    private static Dictionary<string, object> BuildDictionary(IContentType t) => new Dictionary<string, object>
	    {
	        {ContentTypeType.Name.ToString(), t.Name},
	        {ContentTypeType.StaticName.ToString(), t.NameId},
	        {ContentTypeType.Description.ToString(), t.Description},
	        {ContentTypeType.IsDynamic.ToString(), t.IsDynamic},

	        {ContentTypeType.Scope.ToString(), t.Scope},
	        {ContentTypeType.AttributesCount.ToString(), t.Attributes.Count},

	        {ContentTypeType.RepositoryType.ToString(), t.RepositoryType.ToString()},
	        {ContentTypeType.RepositoryAddress.ToString(), t.RepositoryAddress},
	    };
	}
}