using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using ToSic.Eav.Data;
using ToSic.Eav.ImportExport.Internal;
using ToSic.Eav.ImportExport.Internal.Xml;
using ToSic.Testing.Shared;
using ToSic.Testing.Shared.Mocks;

namespace ToSic.Eav.Apps.Tests.ImportExport;

[TestClass]
public class ValueConversionTests: TestBaseForIoC
{
    #region Initialize Test

    protected override IServiceCollection SetupServices(IServiceCollection services) =>
        base.SetupServices(services)
            .AddTransient<ExportImportValueConversion>()
            .AddTransient<IValueConverter, MockValueConverter>();

    #endregion

    //private int AppId = 78;
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
        var exportListXml = GetService<ExportImportValueConversion>();

        string ResolveHyperlinksFromSite(Guid itemGuid, string value, ValueTypes attrType) 
            => exportListXml.ResolveHyperlinksFromSite(itemGuid, value, attrType);

        // test the Resolve Hyperlink
        var link = ResolveHyperlinksFromSite(ItemGuid, link2sic, ValueTypes.Hyperlink);
        Assert.AreEqual(link, link2sic, "real link should stay the same");

        link = ResolveHyperlinksFromSite(ItemGuid, linkFile, ValueTypes.Hyperlink);
        Assert.AreNotEqual(link, linkFile, "file link should change");

        link = ResolveHyperlinksFromSite(ItemGuid, linkPage, ValueTypes.Hyperlink);
        Assert.AreNotEqual(link, linkPage, "page link should change");

        link = ResolveHyperlinksFromSite(ItemGuid, "http://www.2sic.com/", ValueTypes.String);
        Assert.AreEqual(link, link2sic, "non-link shouldn't resolve");

        link = ResolveHyperlinksFromSite(ItemGuid, linkPage, ValueTypes.String);
        Assert.AreEqual(link, linkPage, "non-link shouldn't resolve");
        link = ResolveHyperlinksFromSite(ItemGuid, linkFile, ValueTypes.String);
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
        var ExportListXml = GetService<ExportImportValueConversion>();

        // test resolves on any value, just certainly not a link, with "no-resolve"
        Assert.AreEqual(XmlConstants.NullMarker, ResolveValueTest(ExportListXml, ItemGuid, attrType, null, true), "test null resolve");
        Assert.AreEqual(XmlConstants.EmptyMarker, ResolveValueTest(ExportListXml, ItemGuid, attrType, "", true), "test empty resolve");
        Assert.AreEqual(unchanged, ResolveValueTest(ExportListXml, ItemGuid, attrType, unchanged, true), "test text resolve");
        Assert.AreEqual(link2sic, ResolveValueTest(ExportListXml, ItemGuid, attrType, link2sic, true), "test http: resolve");
        Assert.AreNotEqual(linkFile, ResolveValueTest(ExportListXml, ItemGuid, attrType, linkFile, true), "test file: resolve");
        Assert.AreNotEqual(linkPage, ResolveValueTest(ExportListXml, ItemGuid, attrType, linkPage, true), "test page: resolve");
    }

    private string ResolveValueTest(ExportImportValueConversion converter, Guid itemGuid,
        string attrType, string value, bool resolveLinks)
        => converter.ResolveValue(itemGuid, ValueTypeHelpers.Get(attrType), value, resolveLinks);



    private void TestResolvesWithNonLinkType(string attrType, bool tryResolve)
    {
        var ExportListXml = GetService<ExportImportValueConversion>();

        Assert.AreEqual(XmlConstants.NullMarker, ResolveValueTest(ExportListXml, ItemGuid, attrType, null, tryResolve), "test null resolve");
        Assert.AreEqual(XmlConstants.EmptyMarker, ResolveValueTest(ExportListXml, ItemGuid, attrType, "", tryResolve), "test empty resolve");
        Assert.AreEqual(unchanged, ResolveValueTest(ExportListXml, ItemGuid, attrType, unchanged, tryResolve), "test text resolve");
        Assert.AreEqual(link2sic, ResolveValueTest(ExportListXml, ItemGuid, attrType, link2sic, tryResolve), "test http: resolve");
        Assert.AreEqual(linkFile, ResolveValueTest(ExportListXml, ItemGuid, attrType, linkFile, tryResolve), "test file: resolve");
        Assert.AreEqual(linkPage, ResolveValueTest(ExportListXml, ItemGuid, attrType, linkPage, tryResolve), "test page: resolve");
    }

    #endregion

}