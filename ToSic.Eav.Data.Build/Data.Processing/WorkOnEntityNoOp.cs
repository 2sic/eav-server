using ToSic.Sys.HookUp;

namespace ToSic.Eav.Data.Processing;

/// <summary>
/// WIP - idea is to have objects which can process data - like before/after saving.
/// Specs still very unclear; 2dm.
/// </summary>
[ShowApiWhenReleased(ShowApiMode.Never)]
public class WorkOnEntityNoOp: IWorkEntityAction
{
    /// <summary>
    /// do nothing
    /// </summary>
    /// <param name="context">The work context</param>
    /// <param name="package">The entity / action package</param>
    /// <returns>The processed entity package</returns>
    public virtual Task<Package<IEntity?>> Handle(WorkContext context, Package<DoNamedInput<IEntity?>> package)
    {
        return Task.FromResult(package.Data.Input.ToPackage());
    }
}
