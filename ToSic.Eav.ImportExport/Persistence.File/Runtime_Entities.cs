using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Data;
using ToSic.Eav.Logging;
using static ToSic.Eav.Configuration.FsDataConstants;

namespace ToSic.Eav.Persistence.File
{
    public partial class Runtime
    {
        protected int EntityIdSeed = GlobalEntityIdMin;

        public IEnumerable<IEntity> LoadGlobalItems(string groupIdentifier)
        {
            var wrapLog = Log.Fn<IEnumerable<IEntity>>(groupIdentifier);

            if(!EntityItemFolders.Any(f => f.Equals(groupIdentifier)))
                throw new ArgumentOutOfRangeException(nameof(groupIdentifier), "atm we can only load items of type " + string.Join("/", EntityItemFolders));

            // Get items
            var entities = new List<IEntity>();
            foreach (var l in Loaders)
            {
                entities.AddRange(l.Entities(groupIdentifier, EntityIdSeed));
                EntityIdSeed += GlobalEntitySourceSkip; // update the seed for next rounds or other uses of the seed
            }

            return wrapLog.Return(entities, $"{entities.Count} items of type {groupIdentifier}");
        }
    }
}
