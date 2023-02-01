using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using ToSic.Eav.Configuration;
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
    /// A DataSource that list all features.
    /// </summary>
    [InternalApi_DoNotUse_MayChangeWithoutNotice]
    [VisualQuery(
        NiceName = "Features",
        UiHint = "List all features",
        Icon = Icons.TableChart,
        Type = DataSourceType.System,
        GlobalName = "398d0b9f-044f-48f7-83ef-307872f7ed93",
        Difficulty = DifficultyBeta.Advanced,
        DynamicOut = false
    )]
    // ReSharper disable once UnusedMember.Global
    public sealed class Features : DataSource
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
            // ReSharper disable once UnusedMember.Local
            set => Configuration[AppIdKey] = value.ToString();
        }

        #endregion

        /// <inheritdoc />
        /// <summary>
        /// Constructs a new Scopes DS
        /// </summary>
        [PrivateApi]
        public Features(Dependencies dependencies, IFeaturesInternal featuresService) : base(dependencies, $"{DataSourceConstants.LogPrefix}.Scopes")
        {
            ConnectServices(
                _featuresService = featuresService
            );
            Provide(GetList);
            ConfigMask(AppIdKey);
        }
        private readonly IFeaturesInternal _featuresService;


        private ImmutableArray<IEntity> GetList() => Log.Func(l =>
        {
            Configuration.Parse();

            var appId = OfAppId;

            var features = _featuresService.All;

            var featureBuilder = new DataBuilderQuickWIP(DataBuilder, appId: appId, typeName: "Feature", titleField: Data.Attributes.TitleNiceName);
            var list = features
                .Select(f => featureBuilder.Create(new Dictionary<string, object>
                    {
                        { Data.Attributes.NameIdNiceName, f.NameId },
                        { Data.Attributes.TitleNiceName, f.Name },
                        { "Description", f.Description },
                        { "Enabled", f.Enabled },
                        { "EnabledByDefault", f.EnabledByDefault },
                        { "EnabledReason", f.EnabledReason },
                        { "EnabledReasonDetailed", f.EnabledReasonDetailed },
                        { "EnabledStored", f.EnabledStored },
                        { "Expires", f.Expires },
                        { "ForEditUi", f.ForEditUi },
                        { "License", f.License },
                        { "LicenseEnabled", f.LicenseEnabled },
                        { "Link", f.Link },
                        { "Security", f.Security },
                        { "Public", f.Public }
                    }, 
                    guid: f.Guid, 
                    created: DateTime.Now, 
                    modified: DateTime.Now)
                ).ToImmutableArray();
            
            return (list, $"{list.Length}");
        });
    }
}