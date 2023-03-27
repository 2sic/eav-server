using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Apps;
using ToSic.Eav.Data.Build;
using ToSic.Eav.Data.Raw;
using ToSic.Eav.DataSource;
using ToSic.Eav.DataSource.VisualQuery;
using ToSic.Eav.DataSources.Queries;
using ToSic.Eav.DataSources.Sys.Types;
using ToSic.Lib.DI;
using ToSic.Lib.Documentation;
using ToSic.Lib.Logging;
using static ToSic.Eav.DataSource.DataSourceConstants;

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.DataSources.Sys
{
    /// <inheritdoc />
    /// <summary>
    /// A DataSource that gets all Apps of a zone.
    /// </summary>
    /// <remarks>
    /// * Changed in v15.05 to use the [immutable convention](xref:NetCode.Conventions.Immutable)
    /// * note that the above change is actually a breaking change, but since this is such an advanced DataSource, we assume it's not used in dynamic code.
    /// </remarks>
    [InternalApi_DoNotUse_MayChangeWithoutNotice]
    [VisualQuery(
        NiceName = "Apps",
        UiHint = "Apps of a Zone",
        Icon = Icons.Apps,
        Type = DataSourceType.System,
        NameId = "ToSic.Eav.DataSources.System.Apps, ToSic.Eav.Apps",
        DynamicOut = false,
        Audience = Audience.Advanced,
        ConfigurationType = "fabc849e-b426-42ea-8e1c-c04e69facd9b",
        NameIds = new []
            {
                "ToSic.Eav.DataSources.System.Apps, ToSic.Eav.Apps",
                // not sure if this was ever used...just added it for safety for now
                // can probably remove again, if we see that all system queries use the correct name
                "ToSic.Eav.DataSources.Apps, ToSic.Eav.Apps",
            },
        HelpLink = "https://github.com/2sic/2sxc/wiki/DotNet-DataSource-Apps")]
    // ReSharper disable once UnusedMember.Global
    public sealed class Apps: CustomDataSourceLight
	{

        #region Configuration-properties (no config)

        private const string ZoneIdField = "ZoneId";
        private const string AppsContentTypeName = "App";

	    /// <summary>
	    /// The attribute whose value will be filtered
	    /// </summary>
	    [Configuration(Field = ZoneIdField)]
	    public int OfZoneId => Configuration.GetThis(ZoneId);

        #endregion

        #region Constructor

        /// <inheritdoc />
        /// <summary>
        /// Constructs a new Apps DS
        /// </summary>
        [PrivateApi]
        public Apps(MyServices services, Generator<Eav.Apps.App> appGenerator, IAppStates appStates) : base(services, $"{LogPrefix}.Apps")
        {
            ConnectServices(appGenerator, appStates);
            ProvideOutRaw(
                () => GetDefault(appStates, appGenerator),
                options: () => new DataFactoryOptions(typeName: AppsContentTypeName, titleField: AppType.Name.ToString())
            );
        }

        #endregion

        private IEnumerable<IRawEntity> GetDefault(IAppStates appStates, Generator<Eav.Apps.App> appGenerator) => Log.Func(l =>
        {
            // try to load the content-type - if it fails, return empty list
            var allZones = appStates.Zones;
            if (!allZones.ContainsKey(OfZoneId)) 
                return (EmptyRawList,"fails load content-type");
            
            var zone = allZones[OfZoneId];

            var list = zone.Apps
                .OrderBy(a => a.Key)
                .Select(app =>
                {
                    Eav.Apps.App appObj = null;
                    Guid? guid = null;
                    string error = null;
                    try
                    {
                        appObj = appGenerator.New().Init(new AppIdentity(zone.ZoneId, app.Key), null);
                        // this will get the guid, if the identity is not "default"
                        if (Guid.TryParse(appObj.NameId, out var g)) guid = g;
                    }
                    catch (Exception ex)
                    {
                        error = "Error looking up App: " + ex.Message;
                    }

                    // Assemble the entities
                    var appEnt = new Dictionary<string, object>
                    {
                        { AppType.Id.ToString(), app.Key },
                        { AppType.Name.ToString(), appObj?.Name ?? "error - can't lookup name" },
                        { AppType.Folder.ToString(), appObj?.Folder ?? "" },
                        { AppType.IsHidden.ToString(), appObj?.Hidden ?? false },
                        { AppType.IsDefault.ToString(), app.Key == zone.DefaultAppId },
                        { AppType.IsPrimary.ToString(), app.Key == zone.PrimaryAppId },
                    };
                    if (error != null)
                        appEnt["Error"] = error;

                    var raw = new RawEntity
                    {
                        Id = app.Key,
                        Guid = guid ?? Guid.Empty,
                        Values = appEnt
                    };
                    return raw;
                })
                .Cast<IRawEntity>();

            return (list, $"ok");
        });
    }
}