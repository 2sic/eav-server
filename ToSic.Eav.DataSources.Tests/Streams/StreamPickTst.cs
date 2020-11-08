using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.Apps;
using ToSic.Eav.Core.Tests;
using ToSic.Eav.DataSources;
using ToSic.Eav.DataSources.Queries;
using ToSic.Eav.DataSourceTests.ExternalData;
using ToSic.Eav.LookUp;

namespace ToSic.Eav.DataSourceTests.Streams
{
    [TestClass]
    public class StreamPickTst: EavTestBase
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

            var ds1 = DataTableTst.GeneratePersonSourceWithDemoData(DefaultStreamSize, 1000);
            var ds2 = DataTableTst.GeneratePersonSourceWithDemoData(MoreStreamSize, 2700);
            var ds3 = DataTableTst.GeneratePersonSourceWithDemoData(53, 5300);
            var dsBuild = Resolve<DataSourceFactory>();
            var streamPick = dsBuild.GetDataSource<StreamPick>(new AppIdentity(1, 1), null, ds1.Configuration.LookUps);
            streamPick.Attach(Constants.DefaultStreamName, ds1);
            streamPick.Attach(MoreStream, ds2);
            streamPick.Attach("Lots", ds3);
            return streamPick;
        }
    }
}
