using ToSic.Eav.Apps.Internal.Work;
using ToSic.Eav.Internal.Unknown;

namespace ToSic.Eav.Apps.Integration;

internal sealed class FileSystemLoaderUnknown: ServiceBase, IAppFileSystemLoader, IIsUnknown
{
    public FileSystemLoaderUnknown(WarnUseOfUnknown<FileSystemLoaderUnknown> _): base(LogConstants.FullNameUnknown)
    { }

    // do nothing
    public IAppFileSystemLoader Init(IAppState app) => this;

    public string Path { get; set; }
    public string PathShared { get; set; }

    // do nothing
    public List<InputTypeInfo> InputTypes() => [];
}