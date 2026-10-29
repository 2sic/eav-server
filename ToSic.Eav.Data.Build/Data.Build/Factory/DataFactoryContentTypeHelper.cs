using ToSic.Eav.Data.Build.Sys;
using ToSic.Eav.Data.Sys;

namespace ToSic.Eav.Data.Build;

/// <summary>
/// Handles complexity of determining / retrieving the correct content-type to apply to a Raw-Entity being converted into a real entity.
/// </summary>
/// <param name="codeCtManager"></param>
/// <param name="typeAssembler"></param>
internal class DataFactoryContentTypeHelper(LazySvc<ContentTypesFromCodeManager> codeCtManager, Generator<ContentTypeAssembler, DataAssemblerOptions> typeAssembler)
    : ServiceWithSetup<DataFactoryOptions>("DaF.PctHlp")
{
    /// <summary>
    /// The type of the source object which was used to create the entity.
    /// Should be set on the first Raw-Entity being converted,
    /// as a foundation in case the target ContentType has not yet been determined.
    /// </summary>
    internal Type? TypeFallbackIfNotSet;

    /// <summary>
    /// The preferred EAV Content-Type to attach to the generated entity.
    /// </summary>
    internal IContentType PreferredContentType => field
        ??= GetPreferredContentType();

    /// <summary>
    /// The preferred title field to use for the generated entity.
    /// </summary>
    internal string PreferredTitleFieldName => field
        ??= MyOptions.TitleField.UseFallbackIfNoValue(PreferredContentType.TitleFieldName ?? AttributeNames.TitleNiceName);

    /// <summary>
    /// Determine if the underlying type explicitly has "object" fields, which means that the generated entity should allow unknown value types.
    /// </summary>
    /// <remarks>
    /// Can only return true or null, because downstream it must know if it was explicit or not.
    /// New v22.01
    /// </remarks>
    internal bool? AllowUnknownValueTypes
    {
        get
        {
            if (_allowObjectsTested)
                return field;
            _allowObjectsTested = true;
            return PreferredContentType.Attributes.Any(a => a.Type == ValueTypes.Object)
                ? field = true
                : null;
        }
    }
    private bool _allowObjectsTested;

    /// <summary>
    /// Get the best possible ContentType definition in the current scenario.
    /// </summary>
    /// <returns></returns>
    internal IContentType GetPreferredContentType()
    {
        var l = Log.Fn<IContentType>();
        // Priority 1: If the options have a type, use that
        if (MyOptions.Type is {} type)
            return l.Return(codeCtManager.Value.Get(type), $"Options.Type: {type.Name}");

        // Priority 2: If the options have a TypeName, use that to create a transient type
        if (MyOptions.TypeName is {} typeName)
            return l.Return(typeAssembler.New(new()).Transient(typeName), $"Options.TypeName: {typeName}");

        // Priority 3: Try to find a type based on the source object
        // but only if the source object has an explicit Attribute-Defined type
        // ReSharper disable once InvertIf
        if (TypeFallbackIfNotSet != null && codeCtManager.Value.IsConfigured(TypeFallbackIfNotSet))
        {
            var generatedFromType = codeCtManager.Value.Get(TypeFallbackIfNotSet);
            return l.Return(generatedFromType, $"Generated from type: {generatedFromType.Name}");
        }

        // Priority 9: Use a fallback / auto-generated type
        return l.Return(typeAssembler.New(new()).Transient(DataConstants.DataFactoryDefaultTypeName), "Fallback type");
    }

}
