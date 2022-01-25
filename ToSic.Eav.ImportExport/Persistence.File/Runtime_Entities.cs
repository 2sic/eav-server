using System;
using System.Collections.Generic;
using ToSic.Eav.Configuration;
using ToSic.Eav.Data;

namespace ToSic.Eav.Persistence.File
{
    public partial class Runtime
    {
        private int EntityIdSeed = Global.GlobalEntityIdMin;

        public IEnumerable<IEntity> LoadGlobalItems(string groupIdentifier)
        {
            var wrapLog = Log.Call<IEnumerable<IEntity>>(groupIdentifier);

            if (groupIdentifier != Global.GroupQuery && groupIdentifier != Global.GroupConfiguration)
                throw new ArgumentOutOfRangeException(nameof(groupIdentifier), "atm we can only load items of type 'query'/'configuration'");

            var doQuery = groupIdentifier == Global.GroupQuery;

            // 3 - return content types
            var entities = new List<IEntity>();
            foreach (var l in Loaders)
            {
                entities.AddRange(doQuery ? l.Queries(EntityIdSeed) : l.Configurations(EntityIdSeed));
                EntityIdSeed += Global.GlobalEntitySourceSkip;
            }

            return wrapLog($"{entities.Count} items of type {groupIdentifier}", entities);
        }
    }
}
