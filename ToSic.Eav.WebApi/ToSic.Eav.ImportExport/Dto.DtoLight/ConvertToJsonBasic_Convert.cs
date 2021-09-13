using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Data;
using ToSic.Eav.ImportExport.JsonLight;

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.Convert
{
    public partial class ConvertToJsonLight
    {
        #region Many variations of the Convert-Statement expecting IEntity

        /// <inheritdoc/>
        public IEnumerable<JsonEntity> Convert(IEnumerable<IEntity> entities)
        {
            var wrapLog = Log.Call(useTimer: true);
            var topEntities = MaxItems == 0 ? entities : entities.Take(MaxItems);
            var result = topEntities.Select(GetDictionaryFromEntity).ToList();
            wrapLog("ok");
            return result;
        }

        /// <inheritdoc/>
        public JsonEntity Convert(IEntity entity)
        {
            var wrapLog = Log.Call(useTimer: true);
            var result = entity == null ? null : GetDictionaryFromEntity(entity);
            wrapLog("ok");
            return result;
        }

        #endregion

        #region Dynamic/Objects - which first must detect what it is

        public IEnumerable<JsonEntity> Convert(IEnumerable<object> dynamicList)
        {
            // TODO CONTINUE HERE
            // MUST TEST EVERYTHING


            // 2021-09-11 DataStreams can be handled a bit more efficiently
            if (dynamicList is IEnumerable<IEntity> stream) return Convert(stream);

            return dynamicList
                .Select(c =>
                {
                    IEntity entity = null;
                    if (c is IEntity ent) entity = ent;
                    else if (c is IEntityWrapper dynEnt) entity = dynEnt.Entity;
                    if (entity == null)
                        throw new Exception("tried to convert an item, but it was not a known Entity-type");
                    return GetDictionaryFromEntity(entity);
                })
                .ToList();
        }

        public virtual JsonEntity Convert(object dynamicEntity)
        {
            if (dynamicEntity is IEntity ent) return Convert(ent);
            if (dynamicEntity is IEntityWrapper dynEnt) return Convert(dynEnt.Entity);

            throw new ArgumentException("expected an IEntity or IEntityWrapper like a DynamicEntity, but got something else", nameof(dynamicEntity));
        }

        #endregion

        public IEnumerable<JsonEntity> Convert(IEnumerable<IEntityWrapper> dynamicList)
            => Convert(dynamicList as IEnumerable<object>);

        /// <inheritdoc />
        public JsonEntity Convert(IEntityWrapper dynamicEntity)
            => Convert(dynamicEntity.Entity);
    }
}
