using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToSic.Eav.Run;

namespace ToSic.Eav.Apps.Parts
{
    public class LanguageRuntime
    {
        public LanguageRuntime(Lazy<IZoneMapper> zoneMapperLazy)
        {
            _zoneMapperLazy = zoneMapperLazy;
        }
        private readonly Lazy<IZoneMapper> _zoneMapperLazy;

        public List<LanguageInfo> LanguagesWithPermissions(AppState appState)
        {
            return null;
        }

        public class LanguageInfo
        {
            public string Code { get; }
            public string Name { get; }
            public bool Enabled { get; }
            public bool AllowEdit { get; }
        }
    }
}
