using System.Collections.Generic;
using ToSic.Eav.Interfaces;
using ToSic.Eav.Logging.Simple;

namespace ToSic.Eav.Apps.Parts
{
    public class MetadataRuntime: RuntimeBase
    {
        internal MetadataRuntime(AppRuntime app, Log parentLog) : base(app, parentLog) { }

        public IEnumerable<IEntity> Get<T>(int targetType, T key, string contentTypeName = null)
            => App.Cache.GetMetadata(targetType, key, contentTypeName);


    }
}
