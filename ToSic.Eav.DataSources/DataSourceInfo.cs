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
                VisualQuery.ConfigurationType = string.IsNullOrEmpty(VisualQuery.ConfigurationType) ? FindDataSourceConfiguration(nameof(dsType)) : VisualQuery.ConfigurationType;
            }
        }

        private static string FindDataSourceConfiguration(string name)
        {
            var configurationTypeName = $"{name}Configuration";
            // find the configuration type guid based on the name of DataSource 
            return Guid.Empty.ToString(); // TODO....
        }
    }

}
