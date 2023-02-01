using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using ToSic.Eav.Configuration.Licenses;
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
        NiceName = "Licenses",
        UiHint = "List all licenses",
        Icon = Icons.TableChart,
        Type = DataSourceType.System,
        GlobalName = "402fa226-5584-46d1-a763-e63ba0774c31",
        Difficulty = DifficultyBeta.Advanced,
        DynamicOut = false
    )]
    // ReSharper disable once UnusedMember.Global
    public sealed class Licenses : DataSource
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
        public Licenses(Dependencies dependencies, ILicenseService licenseService) : base(dependencies, $"{DataSourceConstants.LogPrefix}.Scopes")
        {
            ConnectServices(
                _licenseService = licenseService
            );
            Provide(GetList);
            ConfigMask(AppIdKey);
        }
        private readonly ILicenseService _licenseService;


        private ImmutableArray<IEntity> GetList() => Log.Func(l =>
        {
            Configuration.Parse();

            var appId = OfAppId;

            var licenses = _licenseService.All;

            var licenseBuilder = new DataBuilderQuickWIP(DataBuilder, appId: appId, typeName: "License", titleField: Data.Attributes.TitleNiceName);
            var list = licenses
                .Select(lic => licenseBuilder.Create(new Dictionary<string, object>
                    {
                        { Data.Attributes.NameIdNiceName, lic.License.Name},
                        { Data.Attributes.TitleNiceName, lic.Title },
                        { "LicenseKey", lic.LicenseKey },
                        { "LicenseDescription", lic.License.Description },
                        { "LicenseAutoEnable", lic.License.AutoEnable },
                        { "LicensePriority", lic.License.Priority },
                        { "LicenseConditionType", lic.License.Condition.Type },
                        { "LicenseConditionNameId", lic.License.Condition.NameId },
                        { "LicenseConditionIsEnabled", lic.License.Condition.IsEnabled },
                        { "Enabled", lic.Enabled },
                        { "EnabledState", lic.EnabledState },
                        { "Valid", lic.Valid },
                        { "Expiration", lic.Expiration },
                        { "ValidExpired", lic.ValidExpired },
                        { "ValidSignature", lic.ValidSignature },
                        { "ValidFingerprint", lic.ValidFingerprint },
                        { "ValidVersion", lic.ValidVersion },
                        { "Owner", lic.Owner }
                    }, 
                    guid: lic.License.Guid, 
                    created: DateTime.Now, 
                    modified: DateTime.Now)
                ).ToImmutableArray();
            
            return (list, $"{list.Length}");
        });
    }
}