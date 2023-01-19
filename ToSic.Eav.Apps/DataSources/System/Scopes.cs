using System;
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
        NiceName = "beta Scopes",
        UiHint = "list all the data scopes",
        Icon = Icons.Dns, // TODO
        Type = DataSourceType.System,
        GlobalName = "f134e3c1-f09f-4fbc-85be-de43a64c6eed", // new generated
        Difficulty = DifficultyBeta.Advanced,
        DynamicOut = false,
        ExpectsDataOfType = "37b25044-29bb-4c78-85e4-7b89f0abaa2c" // TODO - should have only appId
    )]
    // ReSharper disable once UnusedMember.Global
    public sealed class Scopes : DataSource
    {

        #region Configuration-properties (no config)

        private const string AppIdKey = "AppId";
        private const string AppIdField = "AppId";

        /// <summary>
        /// The app id
        /// </summary>
        public int OfAppId
        {
            get => int.TryParse(Configuration[AppIdKey], out int aid) ? aid : AppId;
            // ReSharper disable once UnusedMember.Global
            set => Configuration[AppIdKey] = value.ToString();
        }

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
            ConfigMask(AppIdKey, $"[Settings:{AppIdField}]");
        }
        private readonly IAppStates _appStates;

        private ImmutableArray<IEntity> GetList()
        {
            var wrapLog = Log.Fn<ImmutableArray<IEntity>>();

            Configuration.Parse();

            var appId = OfAppId;

            var scopes = _appStates.Get(appId).ContentTypes.GetAllScopesWithLabels();

            var builder = DataBuilder;
            var list = scopes
                .OrderByDescending(s => s.Key == Data.Scopes.Default)
                .ThenBy(s => s.Value)
                .Select((s, i) => builder.Entity(
                    new Dictionary<string, object> { { "Name", s.Key }, { "Label", s.Value }, },
                    appId: OfAppId,
                    id: ++i,
                    titleField: "Label",
                    typeName: "EAV_Scopes",
                    guid: Guid.Empty));

            var result = list.ToImmutableArray();
            return wrapLog.Return(result, $"{result.Length}");
        }
    }
}