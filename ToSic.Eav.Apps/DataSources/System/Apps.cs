using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using ToSic.Eav.Apps;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Raw;
using ToSic.Eav.DataSources.Queries;
using ToSic.Eav.DataSources.Sys.Types;
using ToSic.Lib.Logging;
using ToSic.Lib.DI;
using ToSic.Lib.Documentation;
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

        public new class Dependencies: ServiceDependencies<DataSource.Dependencies>
        {
            public Generator<Eav.Apps.App> AppGenerator { get; }
            public IAppStates AppStates { get; }

            public Dependencies(
                DataSource.Dependencies rootDependencies,
                Generator<Eav.Apps.App> appGenerator,
                IAppStates appStates
                ) : base(rootDependencies)
            {
                AddToLogQueue(
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
        public Apps(Dependencies dependencies, IDataBuilder dataBuilder) : base(dependencies.RootDependencies, $"{DataSourceConstants.LogPrefix}.Apps")
        {
            ConnectServices(
                _dataBuilder = dataBuilder.Configure(typeName: AppsContentTypeName, titleField: AppType.Name.ToString())
            );
            ;
            dependencies.SetLog(Log);
            _appGenerator = dependencies.AppGenerator;
            _appStates = dependencies.AppStates;

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

                var appInfo = new AppInfo
                {
                    Id = app.Key,
                    Guid = guid ?? Guid.Empty,
                    Name = appObj?.Name ?? "error - can't lookup name",
                    Folder = appObj?.Folder ?? "",
                    IsHidden = appObj?.Hidden ?? false,
                    IsDefault = app.Key == zone.DefaultAppId,
                    IsPrimary = app.Key == zone.PrimaryAppId,
                };

                if (error != null) 
                    appInfo.Error = error;
                
                return appInfo;

            }).ToList();

            var final = _dataBuilder.CreateMany(list);
            return (final, $"ok");
        });

    }

    /// <summary>
    /// Internal class to hold all the information about the page,
    /// until it's converted to an IEntity in the <see cref="Apps"/> DataSource.
    ///
    /// Important: this is an internal object.
    /// We're just including in in the docs to better understand where the properties come from.
    /// We'll probably move it to another namespace some day.
    /// </summary>
    /// <remarks>
    /// Make sure the property names never change, as they are critical for the created Entity.
    /// </remarks>
    [InternalApi_DoNotUse_MayChangeWithoutNotice]
    public class AppInfo : IRawEntity
    {
        public const string TypeName = "App";
        public int Id { get; set; }
        public Guid Guid { get; set; }
        public string Name { get; set; }
        public string Folder { get; set; }
        public bool IsHidden { get; set; }
        public bool IsDefault { get; set; }
        public bool IsPrimary { get; set; }
        public string Error { get; set; }
        public DateTime Created { get; set; }
        public DateTime Modified { get; set; }

        /// <summary>
        /// Data but without Id, Guid, Created, Modified
        /// </summary>
        [PrivateApi]
        public Dictionary<string, object> RawProperties => new Dictionary<string, object>
        {
            { Data.Attributes.TitleNiceName, Name },
            { nameof(Name), Name },
            { nameof(Folder), Folder },
            { nameof(IsHidden), IsHidden },
            { nameof(IsDefault), IsDefault },
            { nameof(IsPrimary), IsPrimary },
            { nameof(Error), Error },
        };
    }
}