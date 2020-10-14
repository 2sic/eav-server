using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Documentation;

namespace ToSic.Eav.Data
{
    [PrivateApi]
    // ReSharper disable once InconsistentNaming
    public static class IEntityExtensions
    {
        private static int countOneId;
        private static int countOneGuid;
        private static int countOneHas;
        private static int countOneRepo;
        /// <summary>
        /// Get an entity with an entity-id
        /// </summary>
        /// <param name="entities"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public static IEntity One(this IEnumerable<IEntity> entities, int id)
        {
            countOneId++;
            return entities is ImmutableSmartList fastList
                ? fastList.Fast.Get(id)
                : entities.FirstOrDefault(e => e.EntityId == id);
        }

        /// <summary>
        /// get an entity based on the guid
        /// </summary>
        /// <param name="entities"></param>
        /// <param name="guid"></param>
        /// <returns></returns>
        public static IEntity One(this IEnumerable<IEntity> entities, Guid guid)
        {
            countOneGuid++;
            return entities is ImmutableSmartList fastList
                ? fastList.Fast.Get(guid)
                : entities.FirstOrDefault(e => e.EntityGuid == guid);
        }


        /// <summary>
        /// Get an entity based on the repo-id - mainly used for internal APIs and saving/versioning
        /// </summary>
        /// <param name="entities"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public static IEntity FindRepoId(this IEnumerable<IEntity> entities, int id)
        {
            countOneRepo++;
            return entities is ImmutableSmartList fastList
                ? fastList.Fast.GetRepo(id)
                : entities.FirstOrDefault(e => e.RepositoryId == id);
        }

        /// <summary>
        /// Check if an entity is available. 
        /// Mainly used in special cases where published/unpublished are hidden/visible
        /// </summary>
        /// <param name="entities"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public static bool Has(this IEnumerable<IEntity> entities, int id)
        {
            countOneHas++;
            return entities is ImmutableSmartList fastList
                ? fastList.Fast.Has(id)
                : entities.Any(e => e.EntityId == id || e.RepositoryId == id);
        }


        public static Dictionary<string, object> AsDictionary(this IEntity entity)
        {
            var attributes = entity.Attributes.ToDictionary(k => k.Value.Name, v => v.Value[0]);
            attributes.Add("EntityId", entity.EntityId);
            attributes.Add("EntityGuid", entity.EntityGuid);
            return attributes;
        }
    }
}
