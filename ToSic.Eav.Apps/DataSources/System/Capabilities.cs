using System.Linq;
using ToSic.Eav.Configuration;
using ToSic.Eav.Data.Build;
using ToSic.Eav.DataSource;
using ToSic.Eav.DataSource.VisualQuery;
using ToSic.Eav.Run.Capabilities;
using ToSic.Lib.Documentation;

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.DataSources.Sys
{
    /// <inheritdoc />
    /// <summary>
    /// A DataSource that list all features.
    /// </summary>
    [InternalApi_DoNotUse_MayChangeWithoutNotice]
    [VisualQuery(
        NiceName = "Capabilities",
        UiHint = "List all System Capabilities",
        Icon = Icons.TableChart,
        Type = DataSourceType.System,
        NameId = "60d50ed9-846f-4f01-b544-76efe97c94a2",
        Audience = Audience.Advanced,
        DynamicOut = false
    )]
    // ReSharper disable once UnusedMember.Global
    public sealed class Capabilities : CustomDataSource
    {

        #region Configuration-properties (no config)

        #endregion

        /// <inheritdoc />
        /// <summary>
        /// Constructs a new Scopes DS
        /// </summary>
        [PrivateApi]
        public Capabilities(MyServices services, SystemCapabilitiesServices featuresService) : base(services, $"{DataSourceConstants.LogPrefix}.Scopes")
        {
            ConnectServices(featuresService);
            ProvideOutRaw(() => featuresService.All.OrderBy(f => f.NameId), options: () => new DataFactoryOptions(typeName: "Feature"));
        }
    }
}