using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using ToSic.Eav.Apps;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Build;
using ToSic.Eav.DataSource;
using ToSic.Eav.DataSource.VisualQuery;
using ToSic.Lib.Documentation;
using ToSic.Lib.Logging;
using IEntity = ToSic.Eav.Data.IEntity;

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.DataSources.Sys
{
    /// <inheritdoc />
    /// <summary>
    /// A DataSource that list all data scopes.
    /// </summary>
    [InternalApi_DoNotUse_MayChangeWithoutNotice]
    [VisualQuery(
        NiceName = "Data Scopes",
        UiHint = "Data Scopes group Content-Types by topic",
        Icon = Icons.Scopes,
        Type = DataSourceType.System,
        NameId = "f134e3c1-f09f-4fbc-85be-de43a64c6eed",
        Audience = Audience.Advanced,
        DynamicOut = false
    )]
    // ReSharper disable once UnusedMember.Global
    public sealed class Scopes : DataSource
    {
        private readonly IDataFactory _scopesFactory;

        #region Configuration-properties (no config)

        #endregion

        /// <inheritdoc />
        /// <summary>
        /// Constructs a new Scopes DS
        /// </summary>
        [PrivateApi]
        public Scopes(MyServices services, IAppStates appStates, IDataFactory dataFactory) : base(services, $"{DataSourceConstants.LogPrefix}.Scopes")
        {
            ConnectServices(
                _appStates = appStates,
                // Note: these are really the scopes of the current app, so we set the AppId
                _scopesFactory = dataFactory.New(options: new DataFactoryOptions(appId: AppId, typeName: "Scope"))
            );
            ProvideOut(GetList);
        }
        private readonly IAppStates _appStates;

        private IImmutableList<IEntity> GetList() => Log.Func(l =>
        {
            Configuration.Parse();

            var appId = AppId;

            var scopes = _appStates.Get(appId).ContentTypes.GetAllScopesWithLabels();

            var scopeBuilder = _scopesFactory;
            var list = scopes
                .Select(s => scopeBuilder.Create(new Dictionary<string, object>
                    {
                        { Data.Attributes.NameIdNiceName, s.Key },
                        { Data.Attributes.TitleNiceName, s.Value },
                    }
                ))
                .ToImmutableList();

            return (list, $"{list.Count}");
        });
    }
}