using Microsoft.Extensions.DependencyInjection;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Build;
using ToSic.Eav.Data.Mocks;
using ToSic.Eav.Data.Startup;
using ToSic.Eav.ImportExport.Internal;
using ToSic.Eav.ImportExport.Internal.Xml;
using ToSic.Eav.StartUp;
using ToSic.Lib;
using ToSic.Sys;

namespace ToSic.Eav.Apps.Tests.AppValueConversion;

public class ValueConversionTests(ExportImportValueConversion exportListXml)
{
    #region Startup DI Configuration

    public class Startup
    {
        public void ConfigureServices(IServiceCollection services) =>
            services
                .AddTransient<ExportImportValueConversion>()
                .AddTransient<IValueConverter, MockValueConverter>()
                // EAV Core
                //.AddEavDataPersistence()
                .AddEavDataBuild()
                .AddEavCoreLibAndSys()

                .AddEavCoreLibAndSysFallbackServices();
    }

    #endregion

    public Guid ItemGuid = new("cdf540dd-d012-4e7e-b828-7aa0efc5d81f");

    private const string NonLinkType = "whatever";

    private const string Link2Sic = "http://www.2sic.com/", 
        LinkFile = "file:304", 
        LinkPage = "page:44";

    private const string Unchanged = "some test string which shouldn't change in a resolve as it's not a link or reference";


    #region test basic ResolveHyperlink and ResolveValue

    [Fact]
    public void ResolveHyperlinkSiteLink2sic() =>
        // test the Resolve Hyperlink
        Equal(Link2Sic, ResolveHyperlinksFromSite(ItemGuid, Link2Sic, ValueTypes.Hyperlink)); //, "real link should stay the same");

    [Fact]
    public void ResolveHyperlinkSiteLinkFile() =>
        NotEqual(LinkFile, ResolveHyperlinksFromSite(ItemGuid, LinkFile, ValueTypes.Hyperlink)); //, "file link should change");

    [Fact]
    public void ResolveHyperlinkSitePage() =>
        NotEqual(LinkPage, ResolveHyperlinksFromSite(ItemGuid, LinkPage, ValueTypes.Hyperlink)); //, "page link should change");

    [Fact]
    public void ResolveHyperlinkSiteUrl() =>
        Equal(Link2Sic, ResolveHyperlinksFromSite(ItemGuid, "http://www.2sic.com/", ValueTypes.String)); //, "non-link shouldn't resolve");

    [Fact]
    public void ResolveHyperlinkSiteLinkPageString() =>
        Equal(LinkPage, ResolveHyperlinksFromSite(ItemGuid, LinkPage, ValueTypes.String)); //, "non-link shouldn't resolve");

    [Fact]
    public void ResolveHyperlinkSiteLinkFileString() =>
        Equal(LinkFile, ResolveHyperlinksFromSite(ItemGuid, LinkFile, ValueTypes.String)); //, "non-link shouldn't resolve");

    private string ResolveHyperlinksFromSite(Guid itemGuid, string value, ValueTypes attrType) =>
        exportListXml.ResolveHyperlinksFromSite(itemGuid, value, attrType);

    [Fact]
    public void ValueConversion_ResolveValue()
    {
        // test resolves on any value, these versions should always resolve to default values but not resolve links
        TestResolvesWithNonLinkType(NonLinkType, false);
        TestResolvesWithNonLinkType(NonLinkType, true);
        TestResolvesWithNonLinkType(DataTypes.Hyperlink, false);

        var attrType = DataTypes.Hyperlink;

        // test resolves on any value, just certainly not a link, with "no-resolve"
        Equal(XmlConstants.NullMarker, ResolveValueTest(ItemGuid, attrType, null, true));//, "test null resolve");
        Equal(XmlConstants.EmptyMarker, ResolveValueTest(ItemGuid, attrType, "", true));//, "test empty resolve");
        Equal(Unchanged, ResolveValueTest(ItemGuid, attrType, Unchanged, true));//, "test text resolve");
        Equal(Link2Sic, ResolveValueTest(ItemGuid, attrType, Link2Sic, true));//, "test http: resolve");
        NotEqual(LinkFile, ResolveValueTest(ItemGuid, attrType, LinkFile, true));//, "test file: resolve");
        NotEqual(LinkPage, ResolveValueTest(ItemGuid, attrType, LinkPage, true));//, "test page: resolve");
    }

    private string ResolveValueTest(Guid itemGuid, string attrType, string value, bool resolveLinks) =>
        exportListXml.ResolveValue(itemGuid, ValueTypeHelpers.Get(attrType), value, resolveLinks);



    private void TestResolvesWithNonLinkType(string attrType, bool tryResolve)
    {
        Equal(XmlConstants.NullMarker, ResolveValueTest(ItemGuid, attrType, null, tryResolve));//, "test null resolve");
        Equal(XmlConstants.EmptyMarker, ResolveValueTest(ItemGuid, attrType, "", tryResolve));//, "test empty resolve");
        Equal(Unchanged, ResolveValueTest(ItemGuid, attrType, Unchanged, tryResolve));//, "test text resolve");
        Equal(Link2Sic, ResolveValueTest(ItemGuid, attrType, Link2Sic, tryResolve));//, "test http: resolve");
        Equal(LinkFile, ResolveValueTest(ItemGuid, attrType, LinkFile, tryResolve));//, "test file: resolve");
        Equal(LinkPage, ResolveValueTest(ItemGuid, attrType, LinkPage, tryResolve));//, "test page: resolve");
    }

    #endregion

}