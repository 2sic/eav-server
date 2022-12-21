using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using ToSic.Eav.Metadata;
using ToSic.Eav.Persistence.Efc.Models;
using ToSic.Lib.DI;

namespace ToSic.Eav.Persistence.Efc
{
    public class EfcMetadataTargetTypes : ITargetTypes
    {
        #region DI Constructor

        public EfcMetadataTargetTypes(LazySvc<EavDbContext> dbLazy) => _dbLazy = dbLazy;
        private readonly LazySvc<EavDbContext> _dbLazy;

        #endregion

        private const string ReservedType = "Reserved";

        public int GetId(string targetTypeName)
        {
            var found= TargetTypes.FirstOrDefault(mt => mt.Value == targetTypeName);
            if (!found.Equals(default(KeyValuePair<int, string>))) return found.Key;

            // not found yet, check if we got an int and should just return that
            if (int.TryParse(targetTypeName, out var id)) return id;
            throw new ArgumentException($"Tried to find metadata-target name but failed for '{targetTypeName}'", nameof(targetTypeName));
        }

        public string GetName(int typeId)
        {
            if (!TargetTypes.TryGetValue(typeId, out var result))
                throw new ArgumentException(
                    $"Tried to get TargetType name of '{typeId}' but couldn't find it in the list.", nameof(typeId));

            // if it's a reserved key, then future lookups would fail completely (as it exists many times), so in that case 
            // better safely just return the ID
            // note that this isn't perfect for data export/import, but for editing round-trips it's fine
            return result != ReservedType ? result : typeId.ToString();
        }

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
