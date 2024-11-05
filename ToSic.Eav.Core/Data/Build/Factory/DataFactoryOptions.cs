using ToSic.Eav.Plumbing;
using ToSic.Lib.Coding;

namespace ToSic.Eav.Data.Build;

/// <summary>
/// Options how Entities should be generated from the initial data.
/// </summary>
/// <remarks>
/// Added in v15.x but made public in v16.00
/// </remarks>
[PublicApi]
public class DataFactoryOptions
{
    /// <summary>
    /// The App-ID which will be assigned to the generated entities.
    /// By default, it will be `0`
    /// </summary>
    public int AppId { get; }

    /// <summary>
    /// The type name to use for a generated Entity.
    /// </summary>
    public string TypeName { get; }

    /// <summary>
    /// The field in the data which is the default title.
    /// Defaults to `Title` if not set.
    /// </summary>
    public string TitleField { get; }

    /// <summary>
    /// Determines if Zero IDs are auto-incremented - default is `true`.
    /// </summary>
    public bool AutoId { get; }

    public int IdSeed { get; }

    public bool AllowUnknownValueTypes { get; init; }

    public bool WithMetadata { get; init; }

    public Type Type { get; init; }

    /// <summary>
    /// Main Constructor to create such options.
    /// </summary>
    /// <param name="original">Optional initial object which would be used as a template when creating the new one.</param>
    /// <param name="noParamOrder">see [](xref:NetCode.Conventions.NamedParameters)</param>
    /// <param name="appId">Optional AppId to set on each generated Entity</param>
    /// <param name="typeName">Optional ContentTypeName to use instead of the default</param>
    /// <param name="titleField">Optional Title field to use instead of the default `Title`</param>
    /// <param name="autoId">Determines if items with ID 0 should get an automatic ID</param>
    /// <param name="idSeed">A root ID seed for numbering the items</param>
    /// <param name="withMetadata"></param>
    /// 
    public DataFactoryOptions(
        DataFactoryOptions original = default,
        NoParamOrder noParamOrder = default,
        int? appId = default,
        string typeName = default,
        string titleField = default,
        bool? autoId = default,
        int? idSeed = default,
        bool? withMetadata = default,
        Type type = default
    )
    {
        AppId = appId ?? original?.AppId ?? 0;
        TypeName = typeName ?? original?.TypeName ?? DataConstants.DataFactoryDefaultTypeName;
        TitleField = titleField.UseFallbackIfNoValue(original?.TitleField).UseFallbackIfNoValue(Attributes.TitleNiceName);
        AutoId = autoId ?? original?.AutoId ?? true;
        IdSeed = idSeed ?? original?.IdSeed ?? 1;
        AllowUnknownValueTypes = original?.AllowUnknownValueTypes ?? false;
        WithMetadata = withMetadata ?? original?.WithMetadata ?? false;

        Type = type;
    }
}