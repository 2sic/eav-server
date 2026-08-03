using ToSic.Eav.Models.Sys;

namespace ToSic.Eav.Data.Models.Sys;

internal static class DataModelAnalyzerTestAccessors
{
    public static IList<string> FindPriorityTypeNamesTac(Type type)
        => ModelContentTypeNameAnalyzer.FindPriorityTypeNames(null, type, type, null).Names!;
    

    public static IList<string> GetStreamNameListTac<T>()
        where T : class =>
        ModelStreamNames.GetStreamNameList<T>();

    public static Type GetTargetTypeTac<T>()
        where T : class, IModelFromData
        => ModelFromEntityTypeManager.GetTargetType<T>();

    public static Type GetTargetTypeNoFactoryTac<TModel>(string methodName)
        where TModel : class, IModelFromEntity
        => ModelFromEntityTypeManagerNoFactory.GetTargetType<TModel>(methodName);
}