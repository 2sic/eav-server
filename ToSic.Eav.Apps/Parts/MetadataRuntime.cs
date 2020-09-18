using System.Collections.Generic;
using ToSic.Eav.Logging;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.Apps.Parts
{
    public class MetadataRuntime: RuntimeBase
    {
        internal MetadataRuntime(AppRuntime appRt, ILog parentLog) : base(appRt, parentLog) { }

        public IEnumerable<IEntity> Get<T>(int targetType, T key, string contentTypeName = null)
            => AppRT.AppState.Get(targetType, key, contentTypeName);
    }
}
