using ToSic.Eav.Data.Build.Sys;
using ToSic.Eav.Data.Sys;

namespace ToSic.Eav.Data.Build;

internal class DataFactoryPreferredContentType(
    DataFactoryOptions options,
    LazySvc<ContentTypesFromCodeManager> codeCtManager,
    LazySvc<ContentTypeAssembler> typeAssembler,
    ILog parentLog)
    : HelperBase(parentLog, "DaF.PctHlp")
{
    /// <summary>
    /// The type of the source object which was used to create the entity.
    /// Should be set on the first Raw-Entity being converted,
    /// as a foundation in case the target ContentType has not yet been determined.
    /// </summary>
    internal Type? TypeFallbackIfNotSet;

    /// <summary>
    /// Get the best possible ContentType definition in the current scenario.
    /// </summary>
    /// <returns></returns>
    internal IContentType GetPreferredContentType()
    {
        var l = Log.Fn<IContentType>();
        // Priority 1: If the options have a type, use that
        if (options.Type != null)
            return l.Return(codeCtManager.Value.Get(options.Type), $"Options.Type: {options.Type.Name}");

        // Priority 2: If the options have a TypeName, use that to create a transient type
        if (options.TypeName != null)
            return l.Return(typeAssembler.Value.Transient(options.TypeName), $"Options.TypeName: {options.TypeName}");

        // Priority 3: Try to find a type based on the source object
        // but only if the source object has an explicit Attribute-Defined type
        // ReSharper disable once InvertIf
        if (TypeFallbackIfNotSet != null && codeCtManager.Value.IsConfigured(TypeFallbackIfNotSet))
        {
            var generatedFromType = codeCtManager.Value.Get(TypeFallbackIfNotSet);
            return l.Return(generatedFromType, $"Generated from type: {generatedFromType.Name}");
        }

        // Priority 9: Use a fallback / auto-generated type
        return l.Return(typeAssembler.Value.Transient(DataConstants.DataFactoryDefaultTypeName), "Fallback type");
    }

}
