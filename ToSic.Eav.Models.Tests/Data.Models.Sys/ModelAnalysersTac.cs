using ToSic.Eav.Models.Sys;

namespace ToSic.Eav.Data.Models.Sys;

internal static class ModelAnalysersTac
{
    public static IList<string> FindPriorityTypeNamesTac(Type type)
        => ModelContentTypeNameExtractor.GetNames(new(type, type, new(), nameof(FindPriorityTypeNamesTac))).Names;
    
    public static IList<string> FindPriorityTypeNamesTac(ToModelSpecs specs)
        => ModelContentTypeNameExtractor.GetNames(specs).Names;

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