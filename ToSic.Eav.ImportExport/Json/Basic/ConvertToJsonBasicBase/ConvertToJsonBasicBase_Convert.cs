using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Data;

namespace ToSic.Eav.ImportExport.Json.Basic
{
    public abstract partial class ConvertToJsonBasicBase
    {
        #region Many variations of the Convert-Statement expecting various kinds of input

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


    }
}
