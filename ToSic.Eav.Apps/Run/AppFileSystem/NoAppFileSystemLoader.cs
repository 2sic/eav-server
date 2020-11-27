using System.Collections.Generic;
using ToSic.Eav.Apps.Parts;
using ToSic.Eav.Logging;

namespace ToSic.Eav.Apps.Run
{
    public class NoAppFileSystemLoader: IAppFileSystemLoader
    {
        public IAppFileSystemLoader Init(int appId, string path, ILog log)
        {
            // do nothing
            return this;
        }

        public string Path { get; set; }

        public List<InputTypeInfo> InputTypes()
        {
            // do nothing
            return new List<InputTypeInfo>();
        }
    }
}
