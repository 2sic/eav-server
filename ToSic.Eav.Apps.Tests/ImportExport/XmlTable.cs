using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.Apps.ImportExport;
using ToSic.Eav.ImportExport;
using ToSic.Eav.Logging;
using ToSic.Eav.Logging.Simple;
using ToSic.Eav.Persistence.Efc;
using ToSic.Eav.Run;

namespace ToSic.Eav.Apps.Tests.ImportExport
{
    [TestClass]
    public class XmlTable
    {

        public static ILog Log = new Log("TstXml");
        private int AppId = 78;
        public Guid ItemGuid = new Guid("cdf540dd-d012-4e7e-b828-7aa0efc5d81f");

        private const string NonLinkType = "whatever";

        string link2sic = "http://www.2sic.com/", 
            linkFile = "file:304", 
            linkPage = "page:44";

        string unchanged = "some test string which shouldn't change in a resolve as it's not a link or reference";

        [TestMethod]
        [Ignore]
        public void XmlTable_ResolveWithFullFallback()
        {
            var exporter = BuildExporter(AppId, "BlogPost");

            // todo: need ML portal for testing

        }

        #region test basic ResolveHyperlink and ResolveValue
        [TestMethod]
        public void XmlTable_ResolveHyperlink()
        {
            var exportListXml = Factory.Resolve<ExportImportValueConversion>();

            // test the Resolve Hyperlink
            string link = "";
            link = exportListXml.ResolveHyperlinksFromSite(AppId, ItemGuid, link2sic, Constants.DataTypeHyperlink);
            Assert.AreEqual(link, link2sic, "real link should stay the same");

            link = exportListXml.ResolveHyperlinksFromSite(AppId, ItemGuid, linkFile, Constants.DataTypeHyperlink);
            Assert.AreNotEqual(link, linkFile, "file link should change");

            link = exportListXml.ResolveHyperlinksFromSite(AppId, ItemGuid, linkPage, Constants.DataTypeHyperlink);
            Assert.AreNotEqual(link, linkPage, "page link should change");

            link = exportListXml.ResolveHyperlinksFromSite(AppId, ItemGuid, "http://www.2sic.com/", NonLinkType);
            Assert.AreEqual(link, link2sic, "non-link shouldn't resolve");

            link = exportListXml.ResolveHyperlinksFromSite(AppId, ItemGuid, linkPage, NonLinkType);
            Assert.AreEqual(link, linkPage, "non-link shouldn't resolve");
            link = exportListXml.ResolveHyperlinksFromSite(AppId, ItemGuid, linkFile, NonLinkType);
            Assert.AreEqual(link, linkFile, "non-link shouldn't resolve");
        }

        [TestMethod]
        public void XmlTable_ResolveValue()
        {
            // test resolves on any value, these versions should always resolve to default values but not resolve links
            TestResolvesWithNonLinkType(NonLinkType, false);
            TestResolvesWithNonLinkType(NonLinkType, true);
            TestResolvesWithNonLinkType(Constants.DataTypeHyperlink, false);

            var attrType = Constants.DataTypeHyperlink;
            var ExportListXml = Factory.Resolve<ExportImportValueConversion>();

            // test resolves on any value, just certainly not a link, with "no-resolve"
            Assert.AreEqual(XmlConstants.Null, ExportListXml.ResolveValue(AppId, ItemGuid, attrType, null, true), "test null resolve");
            Assert.AreEqual(XmlConstants.Empty, ExportListXml.ResolveValue(AppId, ItemGuid, attrType, "", true), "test empty resolve");
            Assert.AreEqual(unchanged, ExportListXml.ResolveValue(AppId, ItemGuid, attrType, unchanged, true), "test text resolve");
            Assert.AreEqual(link2sic, ExportListXml.ResolveValue(AppId, ItemGuid, attrType, link2sic, true), "test http: resolve");
            Assert.AreNotEqual(linkFile, ExportListXml.ResolveValue(AppId, ItemGuid, attrType, linkFile, true), "test file: resolve");
            Assert.AreNotEqual(linkPage, ExportListXml.ResolveValue(AppId, ItemGuid, attrType, linkPage, true), "test page: resolve");
        }

        private void TestResolvesWithNonLinkType(string attrType, bool tryResolve)
        {
            var ExportListXml = Factory.Resolve<ExportImportValueConversion>();

            Assert.AreEqual(XmlConstants.Null, ExportListXml.ResolveValue(AppId, ItemGuid, attrType, null, tryResolve), "test null resolve");
            Assert.AreEqual(XmlConstants.Empty, ExportListXml.ResolveValue(AppId, ItemGuid, attrType, "", tryResolve), "test empty resolve");
            Assert.AreEqual(unchanged, ExportListXml.ResolveValue(AppId, ItemGuid, attrType, unchanged, tryResolve), "test text resolve");
            Assert.AreEqual(link2sic, ExportListXml.ResolveValue(AppId, ItemGuid, attrType, link2sic, tryResolve), "test http: resolve");
            Assert.AreEqual(linkFile, ExportListXml.ResolveValue(AppId, ItemGuid, attrType, linkFile, tryResolve), "test file: resolve");
            Assert.AreEqual(linkPage, ExportListXml.ResolveValue(AppId, ItemGuid, attrType, linkPage, tryResolve), "test page: resolve");
        }

        #endregion


        private ExportListXml BuildExporter(int appId, string ctName)
        {
            //var dbc = Eav.Factory.Resolve<DbDataController>().Init(null, appId, Log);
            var loader = Factory.Resolve<Efc11Loader>();//.Init(dbc.SqlDb);
            var appPackage = loader.AppState(appId);
            var type = appPackage.ContentTypes.First(ct => ct.Name == ctName);
            return Eav.Factory.Resolve<ExportListXml>().Init(appPackage, type, Log);
        }
    }
}
