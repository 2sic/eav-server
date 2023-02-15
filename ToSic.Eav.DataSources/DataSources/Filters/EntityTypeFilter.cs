﻿using System;
using System.Collections.Immutable;
using System.Linq;
using ToSic.Eav.Apps;
using ToSic.Eav.Data;
using ToSic.Eav.DataSources.Queries;
using ToSic.Lib.Documentation;
using ToSic.Lib.Logging;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.DataSources
{
	/// <inheritdoc />
	/// <summary>
	/// Keep only entities of a specific content-type
	/// </summary>
    [PublicApi_Stable_ForUseInYourCode]
	[VisualQuery(
        NiceName = "Type-Filter",
        UiHint = "Only keep items of the specified type",
        Icon = Icons.RouteAlt,
        Type = DataSourceType.Filter, 
        GlobalName = "ToSic.Eav.DataSources.EntityTypeFilter, ToSic.Eav.DataSources",
        DynamicOut = false,
        In = new[] { Constants.DefaultStreamNameRequired },
	    ExpectsDataOfType = "|Config ToSic.Eav.DataSources.EntityTypeFilter",
        HelpLink = "https://r.2sxc.org/DsTypeFilter")]

    public class EntityTypeFilter : DataSource
	{
        #region Configuration-properties

		/// <summary>
		/// The name of the type to filter for. Either the normal name or the 'StaticName' which is usually a GUID.
		/// </summary>
		[Configuration]
		public string TypeName
        {
            get => Configuration.GetThis();
            set => Configuration.SetThis(value);
        }

        // 2dm 2023-01-22 #maybeSupportIncludeParentApps
        //[PrivateApi("very experimental v15, special edge case")]
        //internal bool IncludeParentApps { get; set; }
        #endregion

        /// <inheritdoc />
        /// <summary>
        /// Constructs a new EntityTypeFilter
        /// </summary>
        [PrivateApi]
        public EntityTypeFilter(IAppStates appStates, Dependencies dependencies) : base(dependencies, $"{DataSourceConstants.LogPrefix}.TypeF")

        {
            _appStates = appStates;
            Provide(GetList);
        }
        private readonly IAppStates _appStates;


        private IImmutableList<IEntity> GetList() => Log.Func(l =>
        {
            Configuration.Parse();
            l.A($"get list with type:{TypeName}");

            // Get original from In-Stream
            if (!GetRequiredInList(out var originals))
                return (originals, "error");

            try
            {
                var appState = _appStates.Get(this);
                var foundType = appState?.GetContentType(TypeName);
                if (foundType != null) // maybe it doesn't find it!
                {
                    var result = originals.OfType(foundType).ToList();
                    // 2dm 2023-01-22 #maybeSupportIncludeParentApps
                    //if (IncludeParentApps)
                    //{
                    //    l.A($"Special internal case - {nameof(IncludeParentApps)}!");
                    //    var parent = appState.ParentApp;
                    //    while (parent?.AppState != null)
                    //    {
                    //        var additions = parent.AppState.List.OfType(foundType);
                    //        result.AddRange(additions);
                    //        parent = parent.AppState.ParentApp;
                    //    }
                    //}
                    return (result.ToImmutableArray(), "fast");
                }
            }
            catch (Exception ex)
            {
                l.Ex(ex);
                /* ignore */
            }

            // This is the fallback, probably slower. In this case, it tries to match the name instead of the real type
            // Reason is that many dynamically created content-types won't be known to the cache, so they cannot be found the previous way

            //if (!GetRequiredInList(out var originals2))
            //    return (originals2, "error");

            return (originals.OfType(TypeName).ToImmutableArray(), "slower");
        });

    }
}