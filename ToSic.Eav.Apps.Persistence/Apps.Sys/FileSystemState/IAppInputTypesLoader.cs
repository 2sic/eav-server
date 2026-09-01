namespace ToSic.Eav.Apps.Sys.FileSystemState;

[ShowApiWhenReleased(ShowApiMode.Never)]
public interface IAppInputTypesLoader: IServiceWithSetup<AppFileSystemLoaderOptions>
{
    /// <summary>
    /// Load all the input types for this app from the folder
    /// </summary>
    /// <returns></returns>
    ICollection<InputTypeInfo> InputTypes();
}