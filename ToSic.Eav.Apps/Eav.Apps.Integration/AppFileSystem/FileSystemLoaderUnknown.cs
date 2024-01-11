using System.Collections.Generic;
using ToSic.Eav.Apps.Work;
using ToSic.Eav.Internal.Unknown;
using ToSic.Lib.Logging;
using ToSic.Lib.Services;

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