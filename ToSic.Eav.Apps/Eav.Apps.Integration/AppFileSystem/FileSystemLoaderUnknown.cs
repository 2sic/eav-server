using ToSic.Eav.Apps.Internal.Work;
using ToSic.Eav.Internal.Unknown;
#pragma warning disable CS9113 // Parameter is unread.

namespace ToSic.Eav.Apps.Integration;

internal sealed class FileSystemLoaderUnknown(WarnUseOfUnknown<FileSystemLoaderUnknown> _) : ServiceBase(LogConstants.FullNameUnknown), IAppFileSystemLoader, IIsUnknown
{

    // do nothing
    public IAppFileSystemLoader Init(IAppState app) => this;

    public string Path { get; set; }
    public string PathShared { get; set; }

    // do nothing
    public List<InputTypeInfo> InputTypes() => [];
}