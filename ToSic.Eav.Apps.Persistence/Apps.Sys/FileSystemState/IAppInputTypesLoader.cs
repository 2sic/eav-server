namespace ToSic.Eav.Apps.Sys.FileSystemState;

[ShowApiWhenReleased(ShowApiMode.Never)]
public interface IAppInputTypesLoader: IHasLog
{
    /// <summary>
    /// Real constructor, after DI
    /// </summary>
    IAppInputTypesLoader Init(IAppReader reader, LogSettings logSettings);

    /// <summary>
    /// Load all the input types for this app from the folder
    /// </summary>
    /// <returns></returns>
    List<InputTypeInfo> InputTypes();
}