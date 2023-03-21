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
            IsGlobal = isGlobal;
            TypeName = dsType.Name;

            VisualQuery = TypeMetadata ?? new VisualQueryAttribute();

            // adjust VisualQuery values for App DataSources
            if (!isGlobal)
            {
                VisualQuery.Type = DataSourceType.App;
                VisualQuery.Icon = string.IsNullOrEmpty(VisualQuery.Icon) ? "star" : VisualQuery.Icon;
            }
        }
    }

}
