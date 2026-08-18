namespace ToSic.Eav.Apps.Sys.Work;

[ShowApiWhenReleased(ShowApiMode.Never)]
public class WorkFieldsDataTypes(): ServiceWithSetup<IAppWorkContext>("Wrk.FDT")
{
    /// <summary>
    /// Get all known data types, like "String", "Number" etc. from DB.
    /// </summary>
    /// <returns></returns>
    public string[] DataTypes()
    {
        var l = Log.Fn<string[]>();
        var result = MyOptions.DbStorage.Attributes.DataTypeNames();
        return l.Return(result, $"{result.Length}");
    }
    
}