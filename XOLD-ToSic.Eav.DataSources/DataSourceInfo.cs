using System;
using System.Linq;
using ToSic.Eav.DataSources.Queries;

namespace ToSic.Eav.DataSources
{
    public class DataSourceInfo
    {
        public Type Type { get; }
        public VisualQueryAttribute VisualQuery { get; }
        public string GlobalName => VisualQuery?.GlobalName;

        public DataSourceInfo(Type dsType)
        {
            Type = dsType;

            // must put this in a try/catch, in case other DLLs have incompatible attributes
            try
            {
                VisualQuery =
                    Type.GetCustomAttributes(typeof(VisualQueryAttribute), false).FirstOrDefault() as
                        VisualQueryAttribute;
            }

            catch {  /*ignore */ }
        }
    }

}
