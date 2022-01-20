using System;
using System.Collections.Generic;
using ToSic.Eav.Run;

namespace ToSic.Eav.Apps.Languages
{
    public class LanguageRuntime
    {
        public LanguageRuntime(Lazy<IZoneMapper> zoneMapperLazy)
        {
            _zoneMapperLazy = zoneMapperLazy;
        }
        private readonly Lazy<IZoneMapper> _zoneMapperLazy;

        public List<AppUserLanguageState> LanguagesWithPermissions(AppState appState)
        {
            return null;
        }
        
    }
}
