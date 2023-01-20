﻿using System.Collections.Generic;
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

        private const string AppIdKey = "AppId";

        /// <summary>
        /// The app id
        /// </summary>
        [PrivateApi("As of now, switching apps is not a feature we want to provide")]
        private int OfAppId
        {
            get => int.TryParse(Configuration[AppIdKey], out var aid) ? aid : AppId;
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
            ConfigMask(AppIdKey);
        }
        private readonly IAppStates _appStates;

        private ImmutableArray<IEntity> GetList() => Log.Func(l =>
        {
            Configuration.Parse();

            var appId = OfAppId;

            var scopes = _appStates.Get(appId).ContentTypes.GetAllScopesWithLabels();

            var scopeBuilder = new DataBuilderQuickWIP(DataBuilder, appId: appId, typeName: "Scope", titleField: "Title");
            var list = scopes
                .Select(s => scopeBuilder.Create(new Dictionary<string, object>
                    {
                        { "NameId", s.Key },
                        { "Title", s.Value },
                    }
                ))
                .ToImmutableArray();

            return (list, $"{list.Length}");
        });
    }
}