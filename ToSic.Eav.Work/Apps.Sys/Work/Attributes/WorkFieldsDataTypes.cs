using ToSic.Eav.Repositories.Sys;
using ToSic.Eav.Repository.Efc.Sys.DbStorage;

namespace ToSic.Eav.Apps.Sys.Work;

[ShowApiWhenReleased(ShowApiMode.Never)]
public class WorkFieldsDataTypes(Generator<DbStorage, StorageOptions> dbGen)
    : ServiceWithSetup<IAppWorkContext>("Wrk.FDT", connect: [dbGen])
{
    /// <summary>
    /// Get all known data types, like "String", "Number" etc. from DB.
    /// </summary>
    /// <returns></returns>
    public string[] DataTypes()
    {
        var l = Log.Fn<string[]>();
        var result = dbGen.New(new(MyOptions.AppReader)).Attributes.DataTypeNames();
        return l.Return(result, $"{result.Length}");
    }
    
}