using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.Apps;
using ToSic.Eav.Data;
using ToSic.Testing.Shared;

namespace ToSic.Eav.Core.Tests.Configuration;

[TestClass]
public class DataOverrideTest : TestBaseDiEavFullAndDb
{
    //public DataOverrideTest() => Build<EavSystemLoader>().LoadLicenseAndFeatures();

    private static Guid fancybox4ItemGuid = new("3356ad17-91ce-4814-83c1-9f527697391a");
    private static Guid fancybox3ItemGuid = new("4b9ef331-480a-4a38-86f1-a580f8345677");

    private const string htmlField = "Html";
    private const string testString = "test-is-override";

    // TODO: @STV - this seems to fail, it appears that the normal data isn't loaded, only system-custom ?
    [TestMethod]
    public void TestNormalFancybox4() => TestWebResourcesExistsOnceAndMayHaveValue(fancybox4ItemGuid, false);

    /// <summary>
    /// This is quite a complex test
    /// - There is an entity in App_Data/system-custom with the same guid as the fancybox3 WebResource
    /// - It has an additional string containing "test-is-override"
    /// - On load, it should _replace_ the original item
    /// - and make sure it's used instead
    /// </summary>
    [TestMethod]
    public void TestOverrideFancybox3() => TestWebResourcesExistsOnceAndMayHaveValue(fancybox3ItemGuid, true);


    private void TestWebResourcesExistsOnceAndMayHaveValue(Guid guid, bool expected)
    {
        var appStates = GetService<IAppReaderFactory>();
        var primaryApp = appStates.GetSystemPreset();

        // Verify there is only one with this guid
        var entities = primaryApp.List.Where(e => e.EntityGuid == guid);
        Assert.AreEqual(1, entities.Count());

        var entity = primaryApp.List.One(guid);
        var html = entity.GetTac<string>(htmlField);

        Assert.AreEqual(expected, html.Contains(testString));
    }


}