using ToSic.Eav.Ui.Context.Sys.Dto;

namespace ToSic.Eav.Ui.Context.Sys;

/// <summary>
/// Service to prepare context information which the UI (admin/edit) typically needs.
/// </summary>
/// <remarks>
/// Relies on <see cref="IUiContextPartsService"/> to provide the specific parts of the context,
/// which can be implemented differently depending on the platform (DNN, Oqtane, etc.)
/// </remarks>
[InternalApi_DoNotUse_MayChangeWithoutNotice]
public interface IUiContextService: IServiceWithSetup<IAppReader?>
{
    /// <summary>
    /// Get the context based on the situation. The caller will use different flags depending on the need.
    /// </summary>
    /// <returns></returns>
    ContextDto Get(Ctx flags, CtxEnable enableFlags);
}