using ToSic.Eav.Code;
using ToSic.Eav.TestData;


namespace ToSic.Eav.DataSourceTests;

internal class AttributeRenameTester(ICanGetService parent)
{
    private DataSourcesTstBuilder DsSvc => field ??= parent.GetService<DataSourcesTstBuilder>();

    public AttributeRename Original;
    public AttributeRename Changed;
    public IEntity CItem;
    public List<IEntity> CList;
    public IEntity OItem;

    public AttributeRenameTester Init(string map, bool preserve = true)
    {
        Original = CreateRenamer(10);
        Changed = CreateRenamer(10);
        if (map != null)
            Changed.AttributeMap = map;
        Changed.KeepOtherAttributes = preserve;

        CList = Changed.ListTac().ToList();
        CItem = CList.First();
        OItem = Original.ListTac().First();
        return this;
    }

    internal void AssertValues(string fieldOriginal, string fieldNew = null)
    {
        var original = OItem;
        var modified = CItem;
        AreNotEqual(original, modified, "This test should never receive the same items!");
        fieldNew = fieldNew ?? fieldOriginal;
        AreEqual(
            original.GetTac<string>(fieldOriginal),
            modified.GetTac<string>(fieldNew), $"Renamed values on field '{fieldOriginal}' should match '{fieldNew}'");

    }

    public AttributeRename CreateRenamer(int testItemsInRootSource)
    {
        var ds = new DataTablePerson(parent).Generate(testItemsInRootSource, 1001);
        var filtered = DsSvc.CreateDataSource<AttributeRename>(ds);
        return filtered;
    }
}