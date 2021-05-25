using System;
using ToSic.Eav.DataSources.Queries;
using ToSic.Eav.Plumbing;

namespace ToSic.Eav.DataSources
{
    public class DataSourceInfo: TypeWithMedataBase<VisualQueryAttribute>
    {
        //public Type Type { get; }
        public VisualQueryAttribute VisualQuery { get; }
        public override string Name => VisualQuery?.GlobalName;

        public DataSourceInfo(Type dsType): base(dsType)
        {
            //Type = dsType;

            //// must put this in a try/catch, in case other DLLs have incompatible attributes
            //try
            //{
            //    VisualQuery =
            //        Type.GetCustomAttributes(typeof(VisualQueryAttribute), false).FirstOrDefault() as
            //            VisualQueryAttribute;
            //}

            //catch {  /*ignore */ }
            VisualQuery = TypeMetadata;
        }
    }

}
