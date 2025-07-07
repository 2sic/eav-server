// Note 2026-07 2dm - I just updated some code to match current API, but the implementation
// looks like it was commented out for more than a year.
// Leave for now, unclear why it was created but then commented out again.
// To reactivate, needs a proper converter for the Features data to IEntity

//using ToSic.Eav.Data.Build;
//using ToSic.Sys.Capabilities.SysFeatures;

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
//        Icon = DataSourceIcons.TableChart,
//        Type = DataSourceType.System,
//        NameId = "60d50ed9-846f-4f01-b544-76efe97c94a2",
//        Audience = Audience.Advanced,
//        DynamicOut = false
//    )]
//    // ReSharper disable once UnusedMember.Global
//    public sealed class SysFeatures : CustomDataSource
//    {

//        /// <inheritdoc />
//        /// <summary>
//        /// Constructs a new Scopes DS
//        /// </summary>
//        [PrivateApi]
//        public SysFeatures(MyServices services, SysFeaturesService sysCapabilities) : base(services, $"{DataSourceConstantsInternal.LogPrefix}.SysCap")
//        {
//            ConnectLogs([sysCapabilities]);
//            ProvideOutRaw(() => sysCapabilities.States.OrderBy(f => f.Aspect.NameId), options: () => new DataFactoryOptions
//            {
//                TypeName = "SystemFeatures"
//            });
//        }
//    }
//}