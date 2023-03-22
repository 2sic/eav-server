using System;
using System.Collections.Immutable;
using System.Linq;
using ToSic.Eav.Apps;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Build;
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
        NameId = "ToSic.Eav.DataSources.System.ContentTypes, ToSic.Eav.Apps",
        Audience = Audience.Advanced,
        DynamicOut = false,
        ConfigurationType = "37b25044-29bb-4c78-85e4-7b89f0abaa2c",
        NameIds = new []
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
        private readonly IDataFactory _dataFactory;

        #region Configuration-properties (no config)

        private const string AppIdKey = "AppId";
	    private const string ContentTypeTypeName = "ContentType";
        private DataFactoryOptions _options = new DataFactoryOptions(typeName: ContentTypeTypeName, titleField: ContentTypeType.Name.ToString());
	    

        /// <summary>
        /// The app id
        /// </summary>
        [Configuration(Field = AppIdKey)]
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
	    [Configuration(Fallback = "Default")]
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
        public ContentTypes(MyServices services, IAppStates appStates, IDataFactory dataFactory): base(services, $"{DataSourceConstants.LogPrefix}.CTypes")
        {
            ConnectServices(
                _appStates = appStates,
                _dataFactory = dataFactory.New(options: new DataFactoryOptions(_options, appId: OfAppId))
            );
            ProvideOut(GetList);
		}
        private readonly IAppStates _appStates;

        private IImmutableList<IEntity> GetList() => Log.Func(l =>
        {
            Configuration.Parse();

            var appId = OfAppId;

            var scp = Scope;
            if (string.IsNullOrWhiteSpace(scp)) scp = Data.Scopes.Default;

            var types = _appStates.Get(appId).ContentTypes.OfScope(scp);

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

                return _dataFactory.Create(ContentTypeUtil.BuildDictionary(t), id: t.Id, guid: guid ?? Guid.Empty);
            });

            var result = list.ToImmutableList();
            return (result, $"{result.Count}");
        });
    }
}