using ToSic.Sys.HookUp;

namespace ToSic.Eav.Ui.Edit.Sys.Load.Prefetch;

/// <summary>
/// Marks a class which can offer additional prefetch information when editing something.
/// </summary>
/// <remarks>
/// Services must be registered with a key, so that the prefetched data can be assigned.
/// </remarks>
public interface IEditLoadPrefetchProvider: IWork<EditLoadDto, object>;
