using System.Collections.Generic;
using ToSic.Eav.DataSources;
using ToSic.Eav.Logging;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.Apps.Parts
{
    public class MetadataRuntime: RuntimeBase
    {
        internal MetadataRuntime(AppRuntime app, ILog parentLog) : base(app, parentLog) { }

        public IEnumerable<IEntity> Get<T>(int targetType, T key, string contentTypeName = null)
            => ((AppRoot)App.Data).Get(targetType, key, contentTypeName);


    }
}
