using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using ToSic.Eav.Apps;
using ToSic.Eav.Data;
using ToSic.Eav.DataSources.Queries;
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
        GlobalName = "f134e3c1-f09f-4fbc-85be-de43a64c6eed",
        Difficulty = DifficultyBeta.Advanced,
        DynamicOut = false
    )]
    // ReSharper disable once UnusedMember.Global
    public sealed class Scopes : DataSource
    {

        #region Configuration-properties (no config)

        #endregion

        /// <inheritdoc />
        /// <summary>
        /// Constructs a new Scopes DS
        /// </summary>
        [PrivateApi]
        public Scopes(Dependencies dependencies, IAppStates appStates) : base(dependencies, $"{DataSourceConstants.LogPrefix}.Scopes")
        {
            ConnectServices(
                _appStates = appStates
            );
            Provide(GetList);
        }
        private readonly IAppStates _appStates;

        private ImmutableArray<IEntity> GetList() => Log.Func(l =>
        {
            Configuration.Parse();

            var appId = AppId;

            var scopes = _appStates.Get(appId).ContentTypes.GetAllScopesWithLabels();

            var scopeBuilder = new DataBuilderQuickWIP(DataBuilder, appId: appId, typeName: "Scope", titleField: Data.Attributes.TitleNiceName);
            var list = scopes
                .Select(s => scopeBuilder.Create(new Dictionary<string, object>
                    {
                        { Data.Attributes.NameIdNiceName, s.Key },
                        { Data.Attributes.TitleNiceName, s.Value },
                    }
                ))
                .ToImmutableArray();

            return (list, $"{list.Length}");
        });
    }
}