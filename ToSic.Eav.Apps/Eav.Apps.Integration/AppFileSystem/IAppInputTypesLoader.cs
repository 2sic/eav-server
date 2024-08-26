using ToSic.Eav.Apps.Internal.Work;

namespace ToSic.Eav.Apps.Integration;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public interface IAppInputTypesLoader: IHasLog
{
    /// <summary>
    /// Real constructor, after DI
    /// </summary>
    /// <param name="app"></param>
    /// <returns></returns>
    IAppInputTypesLoader Init(IAppReader app);

    /// <summary>
    /// Load all the input types for this app from the folder
    /// </summary>
    /// <returns></returns>
    List<InputTypeInfo> InputTypes();
}