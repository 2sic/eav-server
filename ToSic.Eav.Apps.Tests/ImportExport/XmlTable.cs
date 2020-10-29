using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.Apps.ImportExport;
using ToSic.Eav.Data;
using ToSic.Eav.ImportExport;
using ToSic.Eav.Logging;
using ToSic.Eav.Logging.Simple;
using ToSic.Eav.Persistence.Efc;
using ToSic.Eav.Repository.Efc;
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
            var resolver = Factory.Resolve<IValueConverter>();

            // test the Resolve Hyperlink
            string link = "";
            link = ExportListXml.ResolveHyperlinksFromSite(AppId, ItemGuid, link2sic, Constants.DataTypeHyperlink, resolver);
            Assert.AreEqual(link, link2sic, "real link should stay the same");

            link = ExportListXml.ResolveHyperlinksFromSite(AppId, ItemGuid, linkFile, Constants.DataTypeHyperlink, resolver);
            Assert.AreNotEqual(link, linkFile, "file link should change");

            link = ExportListXml.ResolveHyperlinksFromSite(AppId, ItemGuid, linkPage, Constants.DataTypeHyperlink, resolver);
            Assert.AreNotEqual(link, linkPage, "page link should change");

            link = ExportListXml.ResolveHyperlinksFromSite(AppId, ItemGuid, "http://www.2sic.com/", NonLinkType, resolver);
            Assert.AreEqual(link, link2sic, "non-link shouldn't resolve");

            link = ExportListXml.ResolveHyperlinksFromSite(AppId, ItemGuid, linkPage, NonLinkType, resolver);
            Assert.AreEqual(link, linkPage, "non-link shouldn't resolve");
            link = ExportListXml.ResolveHyperlinksFromSite(AppId, ItemGuid, linkFile, NonLinkType, resolver);
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
            var resolver = Factory.Resolve<IValueConverter>();
            // test resolves on any value, just certainly not a link, with "no-resolve"
            Assert.AreEqual(XmlConstants.Null, ExportListXml.ResolveValue(AppId, ItemGuid, attrType, null, true, resolver), "test null resolve");
            Assert.AreEqual(XmlConstants.Empty, ExportListXml.ResolveValue(AppId, ItemGuid, attrType, "", true, resolver), "test empty resolve");
            Assert.AreEqual(unchanged, ExportListXml.ResolveValue(AppId, ItemGuid, attrType, unchanged, true, resolver), "test text resolve");
            Assert.AreEqual(link2sic, ExportListXml.ResolveValue(AppId, ItemGuid, attrType, link2sic, true, resolver), "test http: resolve");
            Assert.AreNotEqual(linkFile, ExportListXml.ResolveValue(AppId, ItemGuid, attrType, linkFile, true, resolver), "test file: resolve");
            Assert.AreNotEqual(linkPage, ExportListXml.ResolveValue(AppId, ItemGuid, attrType, linkPage, true, resolver), "test page: resolve");
        }

        private void TestResolvesWithNonLinkType(string attrType, bool tryResolve)
        {
            var resolver = Factory.Resolve<IValueConverter>();

            Assert.AreEqual(XmlConstants.Null, ExportListXml.ResolveValue(AppId, ItemGuid, attrType, null, tryResolve, resolver), "test null resolve");
            Assert.AreEqual(XmlConstants.Empty, ExportListXml.ResolveValue(AppId, ItemGuid, attrType, "", tryResolve, resolver), "test empty resolve");
            Assert.AreEqual(unchanged, ExportListXml.ResolveValue(AppId, ItemGuid, attrType, unchanged, tryResolve, resolver), "test text resolve");
            Assert.AreEqual(link2sic, ExportListXml.ResolveValue(AppId, ItemGuid, attrType, link2sic, tryResolve, resolver), "test http: resolve");
            Assert.AreEqual(linkFile, ExportListXml.ResolveValue(AppId, ItemGuid, attrType, linkFile, tryResolve, resolver), "test file: resolve");
            Assert.AreEqual(linkPage, ExportListXml.ResolveValue(AppId, ItemGuid, attrType, linkPage, tryResolve, resolver), "test page: resolve");
        }

        #endregion


        private ExportListXml BuildExporter(int appId, string ctName)
        {
            var dbc = DbDataController.Instance(null, appId, Log);
            var loader = new Efc11Loader(dbc.SqlDb);
            var appPackage = loader.AppState(appId);
            var type = appPackage.ContentTypes.First(ct => ct.Name == ctName);
            return new ExportListXml(appPackage, type, Log);
        }
    }
}
