using System.Collections.Immutable;
using System.Linq;
using ToSic.Eav.Interfaces;
using ToSic.Eav.Metadata;
using ToSic.Eav.Persistence.Efc.Models;

namespace ToSic.Eav.Persistence.Efc
{
    public class GlobalMetadataProvider : IGlobalMetadataProvider
    {

        public int GetType(string typeName) => TargetTypes.First(mt => mt.Value == typeName).Key;

        public string GetType(int typeId) => TargetTypes[typeId];

        public ImmutableDictionary<int, string> TargetTypes => _targetTypes ?? (_targetTypes = GetTargetTypes());
        private static ImmutableDictionary<int, string> _targetTypes;

        /// <summary>
        /// this is only needed once per application cycle, as the result is fully cached
        /// </summary>
        /// <returns></returns>
        protected virtual ImmutableDictionary<int, string> GetTargetTypes()
        {
            var dbContext = Factory.Resolve<EavDbContext>();

            return dbContext.ToSicEavAssignmentObjectTypes
                .ToImmutableDictionary(a => a.AssignmentObjectTypeId, a => a.Name);
        }


    }
}
