//using System.Linq;
//using ToSic.Eav.Data.Build;
//using ToSic.Eav.DataSource;
//using ToSic.Eav.DataSource.VisualQuery;
//using ToSic.Eav.Internal.Features;
//using ToSic.Lib.Documentation;

//// ReSharper disable once CheckNamespace
//namespace ToSic.Eav.DataSources.Sys
//{
//    /// <inheritdoc />
//    /// <summary>
//    /// A DataSource that list all features.
//    /// </summary>
//    [PrivateApi("Still WIP")]
//    [VisualQuery(
//        NiceName = "System Capabilities",
//        UiHint = "List all System Capabilities",
//        Icon = Icons.TableChart,
//        Type = DataSourceType.System,
//        NameId = "60d50ed9-846f-4f01-b544-76efe97c94a2",
//        Audience = Audience.Advanced,
//        DynamicOut = false
//    )]
//    // ReSharper disable once UnusedMember.Global
//    public sealed class SysFeatures : CustomDataSource
//    {

//        #region Configuration-properties (no config)

//        #endregion

//        /// <inheritdoc />
//        /// <summary>
//        /// Constructs a new Scopes DS
//        /// </summary>
//        [PrivateApi]
//        public SysFeatures(MyServices services, SysFeaturesService sysCapabilities) : base(services, $"{DataSourceConstants.LogPrefix}.SysCap")
//        {
//            ConnectServices(sysCapabilities);
//            ProvideOutRaw(() => sysCapabilities.States.OrderBy(f => f.Aspect.NameId), options: () => new DataFactoryOptions(typeName: "SystemFeatures"));
//        }
//    }
//}