using System;
using ToSic.Eav.DataSources.Queries;
using ToSic.Eav.Plumbing;

namespace ToSic.Eav.DataSources
{
    public class DataSourceInfo: TypeWithMetadataBase<VisualQueryAttribute>
    {
        public VisualQueryAttribute VisualQuery { get; }

        // By default the name is the global name of the VisualQuery.
        public override string Name => VisualQuery?.GlobalName;

        public bool IsGlobal { get; }

        public string TypeName { get; }

        public DataSourceInfo(Type dsType, bool isGlobal): base(dsType)
        {
            VisualQuery = TypeMetadata;
            IsGlobal = isGlobal;
            TypeName = dsType.Name;
        }
    }

}
