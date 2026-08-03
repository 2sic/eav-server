namespace ToSic.Eav.Models;

[WorkInProgressApi("WIP v22")]
public record ToModelOptions
{
    /// <summary>
    /// The name of the type to match.
    /// Or of the entity type to filter by.
    /// This value is used to select entities of a specific type.
    /// </summary>
    /// <remarks>
    /// Leave `null` for default to just use the type name specified by the model.
    /// Set to <see cref="TypeNameAny"/> (`*`) to allow any type name, effectively disabling the type-name checks.
    /// </remarks>
    public string? TypeName
    {
        get;
#if NETCOREAPP
        init;
#else
        set;
#endif
    }

    public const string TypeNameAny = "*";


    public NullHandling NullHandling
    {
        get;
#if NETCOREAPP
        init;
#else
        set;
#endif
    } = NullHandling.Default;

    [PrivateApi]
    [ShowApiWhenReleased(ShowApiMode.Never)]
    public static ToModelOptions DisableTypeNameCheck = new() { TypeName = TypeNameAny };
}