using System.Collections.Generic;
using ToSic.Eav.Data;
using ToSic.Eav.Interfaces;
using ToSic.Eav.Logging;

namespace ToSic.Eav.Apps.Parts
{
    public class MetadataRuntime: RuntimeBase
    {
        internal MetadataRuntime(AppRuntime app, ILog parentLog) : base(app, parentLog) { }

        public IEnumerable<IEntity> Get<T>(int targetType, T key, string contentTypeName = null)
            => App.Cache.GetMetadata(targetType, key, contentTypeName);


    }
}
