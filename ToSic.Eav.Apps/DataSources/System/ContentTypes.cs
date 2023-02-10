using System;
using System.Collections.Immutable;
using System.Linq;
using ToSic.Eav.Apps;
using ToSic.Eav.Data;
using ToSic.Eav.DataSources.Queries;
using ToSic.Eav.DataSources.Sys.Types;
using ToSic.Lib.Documentation;
using ToSic.Lib.Logging;
using IEntity = ToSic.Eav.Data.IEntity;

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.DataSources.Sys
{
    /// <inheritdoc />
    /// <summary>
    /// A DataSource that all content-types of an app.
    /// </summary>
    [InternalApi_DoNotUse_MayChangeWithoutNotice]
    [VisualQuery(
        NiceName = "Content Types",
        UiHint = "Types of an App",
        Icon = Icons.Dns,
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
    public sealed class ContentTypes: DataSource
	{

        #region Configuration-properties (no config)

        private const string AppIdKey = "AppId";
	    private const string ContentTypeTypeName = "EAV_ContentTypes";
	    

        /// <summary>
        /// The app id
        /// </summary>
        public int OfAppId
        {
            get => Configuration.GetThis(AppId);
            set => Configuration.SetThis(value);
        }

	    /// <summary>
	    /// The scope to get the content types of - normally it's only the default scope
	    /// </summary>
	    /// <remarks>
	    /// * Renamed to `Scope` in v15, previously was called `OfScope`
	    /// </remarks>
	    public string Scope
        {
            get => Configuration.GetThis();
            set => Configuration.SetThis(value);
        }

        [PrivateApi]
        [Obsolete("Do not use anymore, use Scope instead - only left in for compatibility. Probably remove v17 or something")]
	    public string OfScope
        {
            get => Scope;
            set => Scope = value;
        }

        #endregion

        /// <inheritdoc />
        /// <summary>
        /// Constructs a new ContentTypes DS
        /// </summary>
        [PrivateApi]
        public ContentTypes(Dependencies dependencies, IAppStates appStates): base(dependencies, $"{DataSourceConstants.LogPrefix}.CTypes")
        {
            ConnectServices(
                _appStates = appStates
            );
            Provide(GetList);
		    ConfigMaskMyConfig(nameof(OfAppId), AppIdKey);
		    ConfigMask($"{nameof(Scope)}||Default");
		}
        private readonly IAppStates _appStates;

	    private ImmutableArray<IEntity> GetList()
	    {
            var wrapLog = Log.Fn<ImmutableArray<IEntity>>();
            
            Configuration.Parse();

            var appId = OfAppId;

	        var scp = Scope;
            if (string.IsNullOrWhiteSpace(scp)) scp = Data.Scopes.Default;

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

                return builder.Entity(ContentTypeUtil.BuildDictionary(t),
                    appId:OfAppId, 
                    id:t.Id, 
                    titleField: ContentTypeType.Name.ToString(),
                    typeName: ContentTypeTypeName,
                    guid: guid);
	        });

	        var result = list.ToImmutableArray();
            return wrapLog.Return(result, $"{result.Length}");
        }
    }
}