using System.Collections.Immutable;
using System.Linq;
using ToSic.Eav.Caching;
using ToSic.Eav.Configuration;
using ToSic.Eav.Data;
using ToSic.Eav.Documentation;

namespace ToSic.Eav.Apps
{
    public partial class AppState
    {

        /// <summary>
        /// The simple list of <em>all</em> entities, used everywhere
        /// </summary>
        [PrivateApi("WIP 12.03")]
        public IEntity SystemSettings
            => (_systemSettings
                ?? (_systemSettings = new SynchronizedEntityList(this,
                    () => Index.Values.Where(e => e.Type.Is(ConfigurationConstants.ContentTypeSettings)).ToImmutableArray())))
                .List
                .FirstOrDefault();
        private SynchronizedEntityList _systemSettings;

    }
}
