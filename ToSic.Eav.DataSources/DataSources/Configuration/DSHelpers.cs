using ToSic.Eav.LookUp;

namespace ToSic.Eav.DataSources
{
    internal static class DSHelpers
    {
        public static T Init<T>(this T dataSource, ILookUpEngine lookUp) where T : IDataSource
        {
            if (lookUp != null && dataSource.Configuration is DataSourceConfiguration dsConfig)
                dsConfig.LookUpEngine = lookUp;
            return dataSource;
        }
    }
}
