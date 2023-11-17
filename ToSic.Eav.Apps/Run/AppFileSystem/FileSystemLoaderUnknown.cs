using System.Collections.Generic;
using ToSic.Eav.Apps.Parts;
using ToSic.Eav.Apps.Work;
using ToSic.Eav.Internal.Unknown;
using ToSic.Lib.Logging;
using ToSic.Eav.Run;
using ToSic.Eav.Run.Unknown;
using ToSic.Lib.Services;

namespace ToSic.Eav.Apps.Run
{
    public sealed class FileSystemLoaderUnknown: ServiceBase, IAppFileSystemLoader, IIsUnknown
    {
        public FileSystemLoaderUnknown(WarnUseOfUnknown<FileSystemLoaderUnknown> _): base(LogConstants.FullNameUnknown)
        { }

        public IAppFileSystemLoader Init(AppState app)
        {
            // do nothing
            return this;
        }

        public string Path { get; set; }
        public string PathShared { get; set; }

        public List<InputTypeInfo> InputTypes()
        {
            // do nothing
            return new List<InputTypeInfo>();
        }
    }
}
