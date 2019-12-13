using ToSic.Eav.LookUp;

namespace ToSic.Eav.DataSources.Configuration
{
    public static class DSHelpers
    {
        public static T Init<T>(this T dataSource, ILookUpEngine lookUp) where T : IDataSource
        {
            dataSource.Configuration.LookUps = lookUp;
            return dataSource;
        }

        public static T Init<T>(this T dataTarget, IDataSource upstream) where T : IDataTarget
        {
            dataTarget.Attach(upstream);
            return dataTarget;
        }
    }
}
