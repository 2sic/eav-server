namespace ToSic.Eav.ImportExport.Tests;

public class SpecsTestExportSerialize: IAppIdentity
{
    public int ZoneId => 4;
    public int AppId => 3013;

    // public int LanguageId = 0; // 37 is also an option...

    public int TestItemToSerialize = 20864;
    public int TestItemAttributeCount = 4;
    public string TestItemTypeName = "SomethingToSerialize";
    public string TestItemStaticTypeId = "3a0fe1e6-40aa-479a-949c-06bbdcee8a26";
    public string TestItemLinkField = "Link";
    public string TestItemLinkValue = "https://2sxc.org";

    public int ContentBlockWithALotOfItems = 20985;

}