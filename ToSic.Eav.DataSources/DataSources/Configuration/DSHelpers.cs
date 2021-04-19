using ToSic.Eav.LookUp;

namespace ToSic.Eav.DataSources
{
    public static class DSHelpers
    {
        public static T Init<T>(this T dataSource, ILookUpEngine lookUp) where T : IDataSource
        {
            if (lookUp != null && dataSource.Configuration is DataSourceConfiguration dsConfig)
                dsConfig.LookUpEngine = lookUp;
            return dataSource;
        }

        // 2021-03 2dm seems unused
        //public static T Init<T>(this T dataTarget, IDataSource upstream) where T : IDataTarget
        //{
        //    dataTarget.Attach(upstream);
        //    return dataTarget;
        //}
    }
}
