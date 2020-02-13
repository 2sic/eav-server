using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.Apps;
using ToSic.Eav.DataSources.Queries;
using ToSic.Eav.LookUp;
using ToSic.Eav.UnitTests.DataSources;

namespace ToSic.Eav.DataSourceTests.StreamPick
{
    [TestClass]
    public class StreamPickTests
    {
        private const int DefaultStreamSize = 10;
        private const int MoreStreamSize = 27;
        private const string MoreStream = "More";

        [TestMethod]
        public void StreamPickDefault()
        {
            var streamPick = BuildStructure();
            var list = streamPick.List;
            Assert.AreEqual(list.Count(), DefaultStreamSize, "default should have 10");
        }

        [TestMethod]
        public void StreamPickMore()
        {
            var streamPick = BuildStructure();
            streamPick.StreamName = MoreStream;
            var list = streamPick.List;
            Assert.AreEqual(list.Count(), MoreStreamSize, "default should have 27");
        }


        private DataSources.StreamPick BuildStructure()
        {
            // todo: create a test using params...
            var paramsOverride = new LookUpInDictionary(QueryConstants.ParamsLookup, new Dictionary<string, string>
            {
                {"StreamParam", "Lots"}
            });

            var ds1 = DataTableDataSourceTest.GeneratePersonSourceWithDemoData(DefaultStreamSize, 1000);
            var ds2 = DataTableDataSourceTest.GeneratePersonSourceWithDemoData(MoreStreamSize, 2700);
            var ds3 = DataTableDataSourceTest.GeneratePersonSourceWithDemoData(53, 5300);
            var dsBuild = new DataSource(null);
            var streamPick = dsBuild.GetDataSource<DataSources.StreamPick>(new AppIdentity(1, 1), null, ds1.Configuration.LookUps);
            streamPick.Attach(Constants.DefaultStreamName, ds1);
            streamPick.Attach(MoreStream, ds2);
            streamPick.Attach("Lots", ds3);
            return streamPick;
        }
    }
}
