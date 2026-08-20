#pragma warning disable CS9113 // Parameter is unread.

namespace ToSic.Eav.Apps.Sys.FileSystemState;

internal sealed class AppInputTypesLoaderUnknown(WarnUseOfUnknown<AppInputTypesLoaderUnknown> _) : ServiceWithSetup<AppFileSystemLoaderOptions>(LogConstants.FullNameUnknown), IAppInputTypesLoader, IIsUnknown
{
    // do nothing
    public void Init(AppFileSystemLoaderOptions options)
    {
        Log.A("Unknown App Repo loader - won't load anything");
    }

    public string Path { get; set; } = "";

    public string PathShared { get; set; } = "";

    // do nothing
    public ICollection<InputTypeInfo> InputTypes() => [];
}