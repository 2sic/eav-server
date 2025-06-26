namespace ToSic.Eav.Apps.Sys.FileSystemState;

[ShowApiWhenReleased(ShowApiMode.Never)]
public interface IAppInputTypesLoader: IHasLog
{
    /// <summary>
    /// Real constructor, after DI
    /// </summary>
    void Init(IAppReader reader, LogSettings logSettings, string? optionalOverrideAppFolder = default);

    /// <summary>
    /// Load all the input types for this app from the folder
    /// </summary>
    /// <returns></returns>
    ICollection<InputTypeInfo> InputTypes();
}