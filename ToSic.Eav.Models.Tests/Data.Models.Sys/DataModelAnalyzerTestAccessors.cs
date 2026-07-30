using ToSic.Eav.Models.Sys;

namespace ToSic.Eav.Data.Models.Sys;

internal static class DataModelAnalyzerTestAccessors
{
    public static IList<string> GetContentTypeNamesTac(Type type) =>
        DataModelAnalyzer.GetValidTypeNames(type);

    public static IList<string> GetContentTypeNamesTac(Type type, string? preset) =>
        DataModelAnalyzer.GetValidTypeNames(type, preset);

    public static IList<string> GetStreamNameListTac<T>()
        where T : class =>
        DataModelAnalyzer.GetStreamNameList<T>();

    public static Type GetTargetTypeTac<T>()
        where T : class =>
        ModelAnalyseUse.GetTargetType<T>();
}