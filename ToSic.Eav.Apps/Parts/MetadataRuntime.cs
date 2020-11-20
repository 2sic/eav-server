using System.Collections.Generic;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.Apps.Parts
{
    public class MetadataRuntime: PartOf<AppRuntime, MetadataRuntime>
    {
        internal MetadataRuntime() : base("RT.Metadt") { }

        public IEnumerable<IEntity> Get<T>(int targetType, T key, string contentTypeName = null)
            => Parent.AppState.Get(targetType, key, contentTypeName);
    }
}
