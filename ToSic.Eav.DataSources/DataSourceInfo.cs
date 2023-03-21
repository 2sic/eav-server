using System;
using ToSic.Eav.DataSources.Queries;
using ToSic.Eav.Plumbing;

namespace ToSic.Eav.DataSources
{
    public class DataSourceInfo: TypeWithMetadataBase<VisualQueryAttribute>
    {
        
        public VisualQueryAttribute VisualQuery { get; }

        // By default the name is the global name of the VisualQuery.
        public override string Name => VisualQuery?.NameId;

        public bool IsGlobal { get; }

        public string TypeName { get; }

        /// <summary>
        /// Error object WIP
        /// </summary>
        public DataSourceInfoError ErrorOrNull { get; }

        public DataSourceInfo(Type dsType, bool isGlobal, VisualQueryAttribute fallbackVisualQuery = null, DataSourceInfoError error = default) : base(dsType)
        {
            IsGlobal = isGlobal;
            TypeName = dsType.Name;
            VisualQuery = TypeMetadata ?? fallbackVisualQuery;
            ErrorOrNull = error;
        }
    }


    public class DataSourceInfoError
    {
        public string Title { get; set; }
        public string Message { get; set; }
    }
}
