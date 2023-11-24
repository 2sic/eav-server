using System.Collections.Generic;
using ToSic.Eav.Apps.Work;
using ToSic.Lib.Logging;

namespace ToSic.Eav.Apps.Run;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public interface IAppFileSystemLoader: IHasLog
{
    /// <summary>
    /// Real constructor, after DI
    /// </summary>
    /// <param name="app"></param>
    /// <returns></returns>
    IAppFileSystemLoader Init(AppState app);

    string Path { get; set; }
    string PathShared { get; set; }

    /// <summary>
    /// Load all the input types for this app from the folder
    /// </summary>
    /// <returns></returns>
    List<InputTypeInfo> InputTypes();
}