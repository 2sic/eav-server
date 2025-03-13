using ToSic.Eav.ImportExport.Tests.Json;
using ToSic.Eav.Persistence.File;
using Xunit.Abstractions;

namespace ToSic.Eav.ImportExport.Tests.Persistence.File;

public class FileLoaderCtAttrGuid(ITestOutputHelper output, FileSystemLoader loaderRaw, JsonTestHelpers jsonTestHelper) : ServiceBase("test"), IClassFixture<DoFixtureStartup<ScenarioBasic>>
{
    private const string DataFolder = "Persistence.File\\Scenarios\\attrib-guids";
    private const string CtAuthorWithSharedAttributes = "AuthorWithSharedAttributes";
    private const string FFullName = "FullName";
    private const string FKey = "Key";
    private const string FKeyInherited = "KeyInherited";
    private static readonly Guid SourceKeyGuid = new("4e7164a1-72f2-4dee-97ea-55362e55e635");

    [Fact]
    public void LoadCtAndCheckAttributeGuids()
    {
        var cts = new LoaderHelper(DataFolder, Log).LoadAllTypes(loaderRaw, Log);
        // var cts = LoadAllTypes(DataFolder);
        var author = cts.First(ct => ct.Name.Equals(CtAuthorWithSharedAttributes));

        VerifyNormalFullName(author);
        VerifySpecialKey(author);
        VerifySpecialKeyInherited(author);
    }

    private static void VerifySpecialKeyInherited(IContentType author)
    {
        var attKeyInherited = author[FKeyInherited];
        NotNull(attKeyInherited.Guid);
        False(attKeyInherited.SysSettings.Share);
        Equal(SourceKeyGuid, attKeyInherited.SysSettings.InheritMetadataMainGuid);
        True(attKeyInherited.SysSettings.InheritMetadata);
        False(attKeyInherited.SysSettings.InheritNameOfPrimary);
    }

    private static void VerifyNormalFullName(IContentType author)
    {
        // Check an attribute which shouldn't have a guid
        var attFullName = author[FFullName];
        Null(attFullName.Guid);
        Null(attFullName.SysSettings);
    }

    private static void VerifySpecialKey(IContentType author)
    {
        // Verify the Key has a GUID
        var attKey = author[FKey];
        NotNull(attKey.Guid);
        Equal(SourceKeyGuid, attKey.Guid);
        True(attKey.SysSettings.Share);
        False(attKey.SysSettings.InheritMetadata);
    }

    [Fact]
    public void ReExportCtAndPreserveAttributeGuid()
    {
        var cts = new LoaderHelper(DataFolder, Log).LoadAllTypes(loaderRaw, Log);
        var ctAuthor = cts.First(ct => ct.Name.Equals(CtAuthorWithSharedAttributes));

        // Re-json the author and check
        var serializer = jsonTestHelper.SerializerOfApp(Constants.PresetAppId);
        var jsonAuthor = serializer.ToJson(ctAuthor);
        var jsonFullName = jsonAuthor.Attributes.First(a => a.Name.Equals(FFullName));
        Null(jsonFullName.Guid);

        var jsonKey = jsonAuthor.Attributes.First(a => a.Name.Equals(FKey));
        NotNull(jsonKey.Guid);
        Equal(SourceKeyGuid, jsonKey.Guid);
        True(jsonKey.SysSettings.Share);

        var jsonKeyInherited = jsonAuthor.Attributes.First(a => a.Name.Equals(FKeyInherited));
        NotNull(jsonKeyInherited.Guid);
        //Equal(0, jsonKeyInherited.SysSettings.ShareLevel);
        Equal(SourceKeyGuid.ToString(), jsonKeyInherited.SysSettings.InheritMetadataOf);

        // 2025-03-13 2dm changed from True to False, not sure why it was True before - the tests have probably been wrong for years already
        //True(jsonKeyInherited.SysSettings.InheritMetadata);
        False(jsonKeyInherited.SysSettings.InheritMetadata);

        False(jsonKeyInherited.SysSettings.InheritName);

        // Re-content-type
        var authorJsonString = serializer.Serialize(ctAuthor);
        var author2 = serializer.DeserializeContentType(authorJsonString);
        VerifyNormalFullName(author2);
        VerifySpecialKey(author2);
        VerifySpecialKeyInherited(author2);
    }

}