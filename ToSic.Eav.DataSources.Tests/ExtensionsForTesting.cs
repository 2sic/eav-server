using System.Collections.Generic;
using ToSic.Eav.DataSources;

namespace ToSic.Eav.DataSourceTests
{
    public static class ExtensionsForTesting
    {
        /// <summary>
        /// All test code should use this property, to ensure that we don't have a huge usage-count on the In-stream just because of tests
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public static IDictionary<string, IDataStream> InForTests(this IDataTarget target) => target.In;

        public static void AttachForTests(this IDataTarget target, IDataSource source) => target.Attach(source);

        public static void AttachForTests(this IDataTarget target, string streamName, IDataSource dataSource,
            string sourceName = Constants.DefaultStreamName)
            => target.Attach(streamName, dataSource, sourceName);

        public static void AttachForTests(this IDataTarget target, string streamName, IDataStream dataStream)
            => target.Attach(streamName, dataStream);
    }
}
