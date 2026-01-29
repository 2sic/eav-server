using ToSic.Eav.Models.Sys;

namespace ToSic.Eav.Data.Models.Sys;

internal static class DataModelAnalyzerTestAccessors
{
    public static List<string> GetContentTypeNamesTac<T>()
        where T : class =>
        DataModelAnalyzer.GetValidTypeNames<T>();

    public static List<string> GetStreamNameListTac<T>()
        where T : class => 
        DataModelAnalyzer.GetStreamNameList<T>();

    public static Type GetTargetTypeTac<T>()
        where T : class => 
        ModelAnalyseUse.GetTargetType<T>();
}