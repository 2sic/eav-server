using System.Collections.Generic;

namespace ToSic.Eav.Interfaces
{
    public interface IRuntime
    {
        IEnumerable<IContentType> LoadGlobalContentTypes();
    }
}
