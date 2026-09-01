using ToSic.Eav.Data;
using ToSic.Eav.Data.Processing;
using ToSic.Sys.HookUp;
using ToSic.Sys.Users;
using static ToSic.Eav.Data.Processing.DataProcessingEvents;

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously

namespace ToSic.Eav.Metadata.Sys;

/// <summary>
/// Warning: this exact name is used in some metadata of entities.
/// Renaming it would break the protection, unless the data is updated.
/// </summary>
/// <param name="user"></param>
internal class PermissionDataProcessor(IUser user) : ServiceBase("Sec.Process"), IWorkEntityAction
{
    public Task<Package<IEntity?>> Handle(WorkContext context, Package<DoNamedInput<IEntity?>> package)
        => package.Data.Action.ToLowerInvariant() switch
        {
            PreEdit or PreSave => new WorkEntityBlockUsers(user)
                .Handle(context, package.RePackage(new PermissionCheckPayload(package.Data) { ExpectedElevation = UserElevation.SiteAdmin })),
            _ => Task.FromResult(package.Data.Input.ToPackage())
        };
}



public class WorkEntityBlockUsers(IUser user): IWork<PermissionCheckPayload, IEntity?>
{
    public Task<Package<IEntity?>> Handle(WorkContext context, Package<PermissionCheckPayload> package)
        => Task.FromResult(user.GetElevation().IsAtLeast(package.Data.ExpectedElevation)
            ? package.RePackage(package.Data.Input)
            : new()
            {
                Data = null,
                Decision = ResultState.Error,
                Exceptions = [new UnauthorizedAccessException($"User is not authorized to {package.Data.Action} this entity.")]
            });


}


public record PermissionCheckPayload : DoNamedInput<IEntity?>
{
    public PermissionCheckPayload(DoNamedInput<IEntity?> input) : base(input)
    { }

    [SetsRequiredMembers]
    public PermissionCheckPayload(string action, IEntity? input, UserElevation expectedElevation) : base(action, input)
    {
        ExpectedElevation = expectedElevation;
    }

    public required UserElevation ExpectedElevation { get; init; }

}