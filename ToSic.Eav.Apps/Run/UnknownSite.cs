﻿using ToSic.Eav.Context;
using ToSic.Eav.Run;

namespace ToSic.Eav.Apps.Run
{
    public class UnknownSite: ISite
    {
        private const string Unknown = "unknown - please implement the ISite interface to get real values";

        internal const int UnknownSiteId = Constants.NullId;

        /// <summary>
        /// The unknown zone defaults to 2, as #1 is usually reserved for internal stuff
        /// </summary>
        internal const int UnknownZoneId = 2;

        public int Id { get; private set; } = UnknownSiteId;

        public string Url => Unknown;
        public int ZoneId { get; private set; } = UnknownZoneId;

        public string DefaultLanguage => "";

        public ISite Init(int siteId)
        {
            Id = siteId;
            ZoneId = siteId;
            return this;
        }

        public string Name => Unknown;
        public string AppsRootPhysical => Unknown;
        public string AppsRootPhysicalFull => Unknown;
        public string AppsRootLink => Unknown;
        public string ContentPath => Unknown;
    }
}
