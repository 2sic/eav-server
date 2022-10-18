using System;
using ToSic.Eav.DataSources.Queries;
using ToSic.Eav.Plumbing;

namespace ToSic.Eav.DataSources
{
    public class DataSourceInfo: TypeWithMetadataBase<VisualQueryAttribute>
    {
        public VisualQueryAttribute VisualQuery { get; }
        public override string Name => VisualQuery?.GlobalName;

        public DataSourceInfo(Type dsType): base(dsType) => VisualQuery = TypeMetadata;
    }

}
