using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Data;
using static ToSic.Eav.Configuration.FsDataConstants;

namespace ToSic.Eav.Persistence.File
{
    public partial class Runtime
    {
        private int EntityIdSeed = GlobalEntityIdMin;

        public IEnumerable<IEntity> LoadGlobalItems(string groupIdentifier)
        {
            var wrapLog = Log.Call<IEnumerable<IEntity>>(groupIdentifier);

            if(!EntityItemFolders.Any(f => f.Equals(groupIdentifier)))
            //if (groupIdentifier != QueriesFolder && groupIdentifier != ConfigFolder && groupIdentifier != EntitiesFolder)
                throw new ArgumentOutOfRangeException(nameof(groupIdentifier), $"atm we can only load items of type " + string.Join("/", EntityItemFolders));

            // Get items
            var entities = new List<IEntity>();
            foreach (var l in Loaders)
            {
                entities.AddRange(l.Entities(groupIdentifier, EntityIdSeed));
                EntityIdSeed += GlobalEntitySourceSkip;
            }

            return wrapLog($"{entities.Count} items of type {groupIdentifier}", entities);
        }
    }
}
