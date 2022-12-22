using System.Collections.Generic;
using ToSic.Eav.Apps.Parts;
using ToSic.Lib.Logging;
using ToSic.Eav.Run;
using ToSic.Eav.Run.Unknown;

namespace ToSic.Eav.Apps.Run
{
    public sealed class FileSystemLoaderUnknown: HasLog, IAppFileSystemLoader, IIsUnknown
    {
        public FileSystemLoaderUnknown(WarnUseOfUnknown<FileSystemLoaderUnknown> warn): base(LogConstants.FullNameUnknown)
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
