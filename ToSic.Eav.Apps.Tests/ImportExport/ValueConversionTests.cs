using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using ToSic.Eav.Apps.ImportExport;
using ToSic.Eav.Data;
using ToSic.Eav.ImportExport;
using ToSic.Testing.Shared;
using ToSic.Testing.Shared.Mocks;

namespace ToSic.Eav.Apps.Tests.ImportExport
{
    [TestClass]
    public class ValueConversionTests: TestBaseForIoC
    {
        #region Initialize Test

        protected override void SetupServices(IServiceCollection services)
        {
            base.SetupServices(services);
            services.AddTransient<ExportImportValueConversion>();
            services.AddTransient<IValueConverter, MockValueConverter>();
        }

        #endregion

        private int AppId = 78;
        public Guid ItemGuid = new Guid("cdf540dd-d012-4e7e-b828-7aa0efc5d81f");

        private const string NonLinkType = "whatever";

        string link2sic = "http://www.2sic.com/", 
            linkFile = "file:304", 
            linkPage = "page:44";

        string unchanged = "some test string which shouldn't change in a resolve as it's not a link or reference";


        #region test basic ResolveHyperlink and ResolveValue
        [TestMethod]
        public void ValueConversion_ResolveHyperlink()
        {
            var exportListXml = Build<ExportImportValueConversion>();

            // test the Resolve Hyperlink
            string link = "";
            link = exportListXml.ResolveHyperlinksFromSite(AppId, ItemGuid, link2sic, DataTypes.Hyperlink);
            Assert.AreEqual(link, link2sic, "real link should stay the same");

            link = exportListXml.ResolveHyperlinksFromSite(AppId, ItemGuid, linkFile, DataTypes.Hyperlink);
            Assert.AreNotEqual(link, linkFile, "file link should change");

            link = exportListXml.ResolveHyperlinksFromSite(AppId, ItemGuid, linkPage, DataTypes.Hyperlink);
            Assert.AreNotEqual(link, linkPage, "page link should change");

            link = exportListXml.ResolveHyperlinksFromSite(AppId, ItemGuid, "http://www.2sic.com/", NonLinkType);
            Assert.AreEqual(link, link2sic, "non-link shouldn't resolve");

            link = exportListXml.ResolveHyperlinksFromSite(AppId, ItemGuid, linkPage, NonLinkType);
            Assert.AreEqual(link, linkPage, "non-link shouldn't resolve");
            link = exportListXml.ResolveHyperlinksFromSite(AppId, ItemGuid, linkFile, NonLinkType);
            Assert.AreEqual(link, linkFile, "non-link shouldn't resolve");
        }

        [TestMethod]
        public void ValueConversion_ResolveValue()
        {
            // test resolves on any value, these versions should always resolve to default values but not resolve links
            TestResolvesWithNonLinkType(NonLinkType, false);
            TestResolvesWithNonLinkType(NonLinkType, true);
            TestResolvesWithNonLinkType(DataTypes.Hyperlink, false);

            var attrType = DataTypes.Hyperlink;
            var ExportListXml = Build<ExportImportValueConversion>();

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
            var ExportListXml = Build<ExportImportValueConversion>();

            Assert.AreEqual(XmlConstants.Null, ExportListXml.ResolveValue(AppId, ItemGuid, attrType, null, tryResolve), "test null resolve");
            Assert.AreEqual(XmlConstants.Empty, ExportListXml.ResolveValue(AppId, ItemGuid, attrType, "", tryResolve), "test empty resolve");
            Assert.AreEqual(unchanged, ExportListXml.ResolveValue(AppId, ItemGuid, attrType, unchanged, tryResolve), "test text resolve");
            Assert.AreEqual(link2sic, ExportListXml.ResolveValue(AppId, ItemGuid, attrType, link2sic, tryResolve), "test http: resolve");
            Assert.AreEqual(linkFile, ExportListXml.ResolveValue(AppId, ItemGuid, attrType, linkFile, tryResolve), "test file: resolve");
            Assert.AreEqual(linkPage, ExportListXml.ResolveValue(AppId, ItemGuid, attrType, linkPage, tryResolve), "test page: resolve");
        }

        #endregion

    }
}
