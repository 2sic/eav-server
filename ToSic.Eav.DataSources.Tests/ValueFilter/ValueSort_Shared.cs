using System.Collections.Generic;
using ToSic.Eav.DataSourceTests.ExternalData;

namespace ToSic.Eav.DataSources.Tests
{
    internal class ValueSortShared
    {
        private static Dictionary<int, DataTable> _cachedDs = new Dictionary<int, DataTable>();

        public static ValueSort GeneratePersonSourceWithDemoData(int itemsToGenerate = 10, int firstId = 1001, bool useCacheForSpeed = true)
        {
            var ds = DataTableTst.GeneratePersonSourceWithDemoData(itemsToGenerate, firstId);
            var filtered = new DataSource(null).GetDataSource<ValueSort>(ds, ds);
            return filtered;
        }

    }
}
