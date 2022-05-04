using System.Collections.Generic;
using ToSic.Eav.Apps.Parts;
using ToSic.Eav.Logging;

namespace ToSic.Eav.Apps.Run
{
    public interface IAppFileSystemLoader
    {
        /// <summary>
        /// Real constructor, after DI
        /// </summary>
        /// <param name="app"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        IAppFileSystemLoader Init(AppState app, ILog log);

        string Path { get; set; }
        string PathShared { get; set; }

        /// <summary>
        /// Load all the input types for this app from the folder
        /// </summary>
        /// <returns></returns>
        List<InputTypeInfo> InputTypes();
    }
}
