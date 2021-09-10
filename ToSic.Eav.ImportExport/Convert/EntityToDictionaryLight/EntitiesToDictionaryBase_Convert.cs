using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Data;
using ToSic.Eav.ImportExport.Json.V0;

namespace ToSic.Eav.ImportExport.Convert.EntityToDictionaryLight
{
    public abstract partial class EntitiesToDictionaryBase
    {
        #region Many variations of the Convert-Statement expecting various kinds of input

        /// <inheritdoc/>
        public IEnumerable<IJsonEntity> Convert(IEnumerable<IEntity> entities)
        {
            var wrapLog = Log.Call(useTimer: true);
            var topEntities = MaxItems == 0 ? entities : entities.Take(MaxItems);
            var result = topEntities.Select(GetDictionaryFromEntity).ToList();
            wrapLog("ok");
            return result;
        }

        /// <inheritdoc/>
        public IJsonEntity Convert(IEntity entity)
        {
            var wrapLog = Log.Call(useTimer: true);
            var result = entity == null ? null : GetDictionaryFromEntity(entity);
            wrapLog("ok");
            return result;
        }

        #endregion


    }
}
