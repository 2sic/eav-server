using System;
using System.Collections.Immutable;
using System.Linq;
using ToSic.Eav.Metadata;
using ToSic.Eav.Persistence.Efc.Models;

namespace ToSic.Eav.Persistence.Efc
{
    public class EfcMetadataTargetTypes : ITargetTypes
    {
        private readonly Lazy<EavDbContext> _dbLazy;

        public EfcMetadataTargetTypes(Lazy<EavDbContext> dbLazy) => _dbLazy = dbLazy;

        public int GetId(string typeName) => TargetTypes.First(mt => mt.Value == typeName).Key;

        public string GetName(int typeId) => TargetTypes[typeId];

        public ImmutableDictionary<int, string> TargetTypes => _targetTypes ?? (_targetTypes = GetTargetTypes());
        private static ImmutableDictionary<int, string> _targetTypes;

        /// <summary>
        /// this is only needed once per application cycle, as the result is fully cached
        /// </summary>
        /// <returns></returns>
        protected virtual ImmutableDictionary<int, string> GetTargetTypes()
        {
            return _dbLazy.Value.ToSicEavAssignmentObjectTypes
                .ToImmutableDictionary(a => a.AssignmentObjectTypeId, a => a.Name);
        }


    }
}
