using ToSic.Eav.Data.Raw;
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

    /// <summary>
    /// Optional special options which create-raw might use.
    /// </summary>
    [field: AllowNull, MaybeNull]
    public RawConvertOptions RawConvertOptions
    {
        get => field ??= new();
#if NETFRAMEWORK // #DnnNoInit - DNN uses c# 8 so it doesn't support init
        set => field = value;
#else
        init => field = value;
#endif
    }
}