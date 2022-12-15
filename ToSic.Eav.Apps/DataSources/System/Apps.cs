﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using ToSic.Eav.Apps;
using ToSic.Eav.DataSources.Queries;
using ToSic.Eav.DataSources.Sys.Types;
using ToSic.Eav.DI;
using ToSic.Eav.Documentation;
using ToSic.Lib.Logging;
using ToSic.Eav.Plumbing;
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
    public sealed class Apps: DataSourceBase
	{
        #region Configuration-properties (no config)
	    public override string LogId => "DS.EavAps";

        private const string ZoneKey = "Zone";
        private const string ZoneIdField = "ZoneId";
        private const string AppsContentTypeName = "EAV_Apps";

	    /// <summary>
	    /// The attribute whose value will be filtered
	    /// </summary>
	    public int OfZoneId
	    {
	        get => int.TryParse(Configuration[ZoneKey], out int zid) ? zid : ZoneId;
            // ReSharper disable once UnusedMember.Global
            set => Configuration[ZoneKey] = value.ToString();
	    }

	    #endregion

        /// <inheritdoc />
        /// <summary>
        /// Constructs a new Apps DS
        /// </summary>
        [PrivateApi]
        public Apps(Generator<Eav.Apps.App> appGenerator, IAppStates appStates)
		{
            _appGenerator = appGenerator;
            _appStates = appStates;
            Provide(GetList);
            ConfigMask(ZoneKey, $"[Settings:{ZoneIdField}]");
		}
        private readonly Generator<Eav.Apps.App> _appGenerator;
        private readonly IAppStates _appStates;

        private ImmutableArray<IEntity> GetList()
        {
            var wrapLog = Log.Fn<ImmutableArray<IEntity>>();
            
            Configuration.Parse();
            var builder = DataBuilder;

            // try to load the content-type - if it fails, return empty list
            var zones = _appStates.Zones;
            if (!zones.ContainsKey(OfZoneId)) return ImmutableArray<IEntity>.Empty;
            var zone = zones[OfZoneId];

	        var list = zone.Apps.OrderBy(a => a.Key).Select(app =>
	        {
	            Eav.Apps.App appObj = null;
	            Guid? guid = null;
                string error = null;
                try
                {
                    appObj = _appGenerator.New().Init(new AppIdentity(zone.ZoneId, app.Key), null, Log);
                    // this will get the guid, if the identity is not "default"
                    if (Guid.TryParse(appObj.NameId, out var g)) guid = g;
                }
                catch(Exception ex)
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
                if(error != null)
                    appEnt["Error"] = error;

                var result = builder.Entity(appEnt,
                    appId: app.Key,
                    id: app.Key,
                    titleField: AppType.Name.ToString(),
                    typeName: AppsContentTypeName, 
                    guid: guid);
                return result;
            });

            var final = list.ToImmutableArray();
            return wrapLog.Return(final, $"{final.Length}");
        }

	}
}