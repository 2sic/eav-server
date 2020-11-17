using System.Collections.Generic;
using ToSic.Eav.DataSources;
using ToSic.Eav.DataSourceTests.TestData;

namespace ToSic.Eav.DataSourceTests
{
    internal class ValueSortShared
    {
        private static Dictionary<int, DataTable> _cachedDs = new Dictionary<int, DataTable>();

        public static ValueSort GeneratePersonSourceWithDemoData(int itemsToGenerate = 10, int firstId = 1001, bool useCacheForSpeed = true)
        {
            var ds = DataTablePerson.Generate(itemsToGenerate, firstId);
            var filtered = Factory.Resolve<DataSourceFactory>().GetDataSource<ValueSort>(ds, ds);
            return filtered;
        }

    }
}
