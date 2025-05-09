using ToSic.Eav.Plumbing;

namespace ToSic.Eav.Data.Build;

/// <summary>
/// Options how Entities should be generated from the initial data.
/// </summary>
/// <remarks>
/// > [!TIP]
/// > This is a record since v19.01, which makes it simpler to use and understand.
/// > all the properties are `init` only, but the docs and the DNN DLLs require that they are `set` as well.
/// > This will not be supported in future, so ensure that you only set the properties on `init`.
/// 
/// History
/// 
/// * Added in v15.x but made public in v16.00
/// * Changed to be a record in v19.01 to make things simpler; this could be a breaking change, because the old constructor was removed.
/// </remarks>
[PublicApi]
public record DataFactoryOptions
{
    /// <summary>
    /// The App-ID which will be assigned to the generated entities.
    /// By default, it will be `0`
    /// </summary>
    public int AppId
    {
        get;
#if NETFRAMEWORK
        set;
#else
        init;
#endif
    }

    /// <summary>
    /// The type name to use for a generated Entity.
    /// </summary>
    public string? TypeName
    {
        get => field ?? DataConstants.DataFactoryDefaultTypeName;
#if NETFRAMEWORK
        set => field = value;
#else
        init => field = value;
#endif

    }

    /// <summary>
    /// The field in the data which is the default title.
    /// Defaults to `Title` if not set.
    /// </summary>
    [field: AllowNull, MaybeNull]
    public string TitleField
    {
        get => field.UseFallbackIfNoValue(Attributes.TitleNiceName);
#if NETFRAMEWORK // #DnnNoInit - DNN uses c# 8 so it doesn't support init
        set => field = value;
#else
        init => field = value;
#endif
    }

    /// <summary>
    /// Determines if Zero IDs are auto-incremented. Default is `true`.
    /// </summary>
    public bool AutoId
    {
        get;
#if NETFRAMEWORK // #DnnNoInit - DNN uses c# 8 so it doesn't support init
        set;
#else
        init;
#endif
    } = true;

    /// <summary>
    /// ID seed for all items which must be auto-numbered. Default is `1`.
    /// If the seed is `-1` or less, then it will automatically decrement the count.
    /// This is to clearly indicate virtual IDs which will not be the same in the next run.
    /// </summary>
    public int IdSeed
    {
        get;
#if NETFRAMEWORK // #DnnNoInit - DNN uses c# 8 so it doesn't support init
        set;
#else
        init;
#endif
    } = 1;

    public bool AllowUnknownValueTypes
    {
        get;
#if NETFRAMEWORK // #DnnNoInit - DNN uses c# 8 so it doesn't support init
        set;
#else
        init;
#endif
    }

    public bool WithMetadata
    {
        get;
#if NETFRAMEWORK // #DnnNoInit - DNN uses c# 8 so it doesn't support init
        set;
#else
        init;
#endif
    }

    public Type? Type
    {
        get;
#if NETFRAMEWORK // #DnnNoInit - DNN uses c# 8 so it doesn't support init
        set;
#else
        init;
#endif
    }

    //Important: Changed this to be a record in v19.01.

    //Could be a breaking change, if this constructor below was used anywhere.Will assume that's not the case,

    //but keep this comment in till 2024-07, then remove all this.
    ///// <summary>
    ///// Main Constructor to create such options.
    ///// </summary>
    ///// <param name="original">Optional initial object which would be used as a template when creating the new one.</param>
    ///// <param name="noParamOrder">see [](xref:NetCode.Conventions.NamedParameters)</param>
    ///// <param name="appId">Optional AppId to set on each generated Entity</param>
    ///// <param name="typeName">Optional ContentTypeName to use instead of the default</param>
    ///// <param name="titleField">Optional Title field to use instead of the default `Title`</param>
    ///// <param name="autoId">Determines if items with ID 0 should get an automatic ID</param>
    ///// <param name="idSeed">A root ID seed for numbering the items</param>
    ///// <param name="withMetadata"></param>
    ///// 
    //public DataFactoryOptions(
    //    DataFactoryOptions original = default,
    //NoParamOrder noParamOrder = default,
    //int? appId = default,
    //string typeName = default,
    //string titleField = default,
    //bool? autoId = default,
    //int? idSeed = default,
    //bool? withMetadata = default,
    //Type type = default
    //) : this()
    //{
    //    AppId = appId ?? original?.AppId ?? 0;
    //    TypeName = typeName ?? original?.TypeName ?? DataConstants.DataFactoryDefaultTypeName;
    //    TitleField = titleField.UseFallbackIfNoValue(original?.TitleField).UseFallbackIfNoValue(Attributes.TitleNiceName);
    //    AutoId = autoId ?? original?.AutoId ?? true;
    //    IdSeed = idSeed ?? original?.IdSeed ?? 1;
    //    AllowUnknownValueTypes = original?.AllowUnknownValueTypes ?? false;
    //    WithMetadata = withMetadata ?? original?.WithMetadata ?? false;
    //    Type = type;
    //}
}