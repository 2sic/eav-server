﻿using System.Collections.Generic;
using ToSic.Eav.Apps.Languages;
using ToSic.Eav.Context;
using ToSic.Eav.Documentation;
using ToSic.Eav.Logging;

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.Run
{
    /// <summary>
    /// This helps find Zone information of a Site and the other way around. 
    /// </summary>
    [PrivateApi]
    public interface IZoneMapper: IHasLog<IZoneMapper>
    {
        /// <summary>
        /// Get the primary zoneId which belongs to the site.
        /// </summary>
        int GetZoneId(int siteId);

        /// <summary>
        /// Find the site of a Zone
        /// </summary>
        ISite SiteOfZone(int zoneId);

        /// <summary>
        /// Find the site of an App
        /// </summary>
        ISite SiteOfApp(int appId);

        /// <summary>
        /// The cultures available on this tenant/zone combination
        /// the zone is necessary to determine what is enabled/disabled
        /// </summary>
        /// <returns></returns>
        List<ISiteLanguageState> CulturesWithState(ISite site);
    }
}
