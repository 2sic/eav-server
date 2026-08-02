using ToSic.Eav.Models.Sys;

namespace ToSic.Eav.Data.Models.Sys;

internal static class DataModelAnalyzerTestAccessors
{
    public static IList<string> FindPriorityTypeNamesTac(Type type)
        => DataModelAnalyzer.FindPriorityTypeNames(null, type, type, null).Names!;
    

    public static IList<string> GetStreamNameListTac<T>()
        where T : class =>
        DataModelAnalyzer.GetStreamNameList<T>();

    public static Type GetTargetTypeTac<T>()
        where T : class =>
        ModelAnalyseUse.GetTargetType<T>();
}