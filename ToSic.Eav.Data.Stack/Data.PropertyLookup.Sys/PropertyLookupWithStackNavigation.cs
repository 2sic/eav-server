using ToSic.Eav.Data.Debug;
using ToSic.Lib.Data;

namespace ToSic.Eav.Data.PropertyLookup;

/// <summary>
/// Test code!
/// </summary>
[PrivateApi]
[ShowApiWhenReleased(ShowApiMode.Never)]
public class PropertyLookupWithStackNavigation(PropertyLookupDictionary current, StackAddress stackAddress)
    : Wrapper<PropertyLookupDictionary>(current), IPropertyLookup, IPropertyStackLookup
{
    internal readonly PropertyStackNavigator Navigator = new(current, stackAddress);


    public PropReqResult FindPropertyInternal(PropReqSpecs specs, PropertyLookupPath path)
        => FindPropertyInternalOfStackWrapper(this, specs, path, EavLogs.Eav + ".PrpNav", $"Source: {GetContents().NameId}");

    public PropReqResult GetNextInStack(PropReqSpecs specs, int startAtSource, PropertyLookupPath path) 
        => Navigator.GetNextInStack(specs, startAtSource, path);

    // #DropUseOfDumpProperties
    //public List<PropertyDumpItem> _DumpNameWipDroppingMostCases(PropReqSpecs specs, string path)
    //    => GetContents()._DumpNameWipDroppingMostCases(specs, path);



    /// <summary>
    /// Shared method for other stack wrappers - will log and call the code
    /// </summary>
    public static PropReqResult FindPropertyInternalOfStackWrapper<T>(T parent, PropReqSpecs specs, PropertyLookupPath path, string logName, string logMessage) where T: IPropertyStackLookup
    {
        specs = specs.SubLog(logName);
        var l = specs.LogOrNull.Fn<PropReqResult>($"{logMessage}, {specs.Dump()}");
        var result = parent.GetNextInStack(specs, 0, path);
        return l.Return(result, result?.Result != null ? "found" : null);
    }
}