using ToSic.Sys.HookUp;

namespace ToSic.Eav.Data.Processing;

/// <summary>
/// WIP - idea is to have objects which can process data - like before/after saving.
/// Specs still very unclear; 2dm.
/// </summary>
[PrivateApi("WIP v21")]
[ShowApiWhenReleased(ShowApiMode.Never)]
public interface IWorkEntityAction: IWork<DoNamedInput<IEntity?>, IEntity?>;
