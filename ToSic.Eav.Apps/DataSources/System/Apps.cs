using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using ToSic.Eav.Apps;
using ToSic.Eav.Data;
using ToSic.Eav.DataSources.Queries;
using ToSic.Eav.DataSources.Sys.Types;
using ToSic.Lib.DI;
using ToSic.Lib.Documentation;
using ToSic.Lib.Logging;
using ToSic.Lib.Services;
using IEntity = ToSic.Eav.Data.IEntity;

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.DataSources.Sys
{
    /// <inheritdoc />
    /// <summary>
    /// A DataSource that gets all Apps of a zone.
    /// </summary>
    [InternalApi_DoNotUse_MayChangeWithoutNotice]
    [VisualQuery(
        NiceName = "Apps",
        UiHint = "Apps of a Zone",
        Icon = Icons.Apps,
        Type = DataSourceType.System,
        GlobalName = "ToSic.Eav.DataSources.System.Apps, ToSic.Eav.Apps",
        DynamicOut = false,
        Difficulty = DifficultyBeta.Advanced,
        ExpectsDataOfType = "fabc849e-b426-42ea-8e1c-c04e69facd9b",
        PreviousNames = new []
            {
                "ToSic.Eav.DataSources.System.Apps, ToSic.Eav.Apps",
                // not sure if this was ever used...just added it for safety for now
                // can probably remove again, if we see that all system queries use the correct name
                "ToSic.Eav.DataSources.Apps, ToSic.Eav.Apps",
            },
        HelpLink = "https://github.com/2sic/2sxc/wiki/DotNet-DataSource-Apps")]
    // ReSharper disable once UnusedMember.Global
    public sealed class Apps: DataSource
	{
        private readonly IDataBuilder _dataBuilder;

        #region Configuration-properties (no config)

        private const string ZoneIdField = "ZoneId";
        private const string AppsContentTypeName = "EAV_Apps";

	    /// <summary>
	    /// The attribute whose value will be filtered
	    /// </summary>
	    [Configuration(Field = ZoneIdField)]
	    public int OfZoneId
	    {
	        get => Configuration.GetThis(ZoneId);
            set => Configuration.SetThis(value);
        }

        #endregion

        #region Constructor

        public new class MyServices: MyServicesBase<DataSource.MyServices>
        {
            public Generator<Eav.Apps.App> AppGenerator { get; }
            public IAppStates AppStates { get; }

            public MyServices(
                DataSource.MyServices parentServices,
                Generator<Eav.Apps.App> appGenerator,
                IAppStates appStates
                ) : base(parentServices)
            {
                ConnectServices(
                    AppGenerator = appGenerator,
                    AppStates = appStates
                );
            }
        }
        
        /// <inheritdoc />
        /// <summary>
        /// Constructs a new Apps DS
        /// </summary>
        [PrivateApi]
        public Apps(MyServices services, IDataBuilder dataBuilder) : base(services, $"{DataSourceConstants.LogPrefix}.Apps")
        {
            ConnectServices(
                _dataBuilder = dataBuilder.Configure(typeName: AppsContentTypeName, titleField: AppType.Name.ToString())
            );

            _appGenerator = services.AppGenerator;
            _appStates = services.AppStates;

            Provide(GetList);
        }
        private readonly Generator<Eav.Apps.App> _appGenerator;
        private readonly IAppStates _appStates;

        #endregion

        private IImmutableList<IEntity> GetList() => Log.Func(l =>
        {
            Configuration.Parse();

            // try to load the content-type - if it fails, return empty list
            var zones = _appStates.Zones;
            if (!zones.ContainsKey(OfZoneId)) 
                return (new List<IEntity>().ToImmutableList(),"fails load content-type");
            
            var zone = zones[OfZoneId];

            var list = zone.Apps.OrderBy(a => a.Key).Select(app =>
            {
                Eav.Apps.App appObj = null;
                Guid? guid = null;
                string error = null;
                try
                {
                    appObj = _appGenerator.New().Init(new AppIdentity(zone.ZoneId, app.Key), null);
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
                    {AppType.Id.ToString(), app.Key},
                    {AppType.Name.ToString(), appObj?.Name ?? "error - can't lookup name"},
                    {AppType.Folder.ToString(), appObj?.Folder ?? "" },
                    {AppType.IsHidden.ToString(), appObj?.Hidden ?? false },
                    {AppType.IsDefault.ToString(), app.Key == zone.DefaultAppId},
                    {AppType.IsPrimary.ToString(), app.Key == zone.PrimaryAppId},
                };
                if (error != null)
                    appEnt["Error"] = error;

                var result = _dataBuilder.Create(appEnt, id: app.Key, guid: guid ?? Guid.Empty);
                return result;

            }).ToImmutableList();

            return (list, $"ok");
        });
    }
}