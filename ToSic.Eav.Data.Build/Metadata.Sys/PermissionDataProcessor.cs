using ToSic.Eav.Data;
using ToSic.Eav.Data.Processing;
using ToSic.Sys.HookUp;
using ToSic.Sys.Users;
using static ToSic.Eav.Data.Processing.DataProcessingEvents;

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously

namespace ToSic.Eav.Metadata.Sys;

internal class PermissionDataProcessor(IUser user) : ServiceBase("Sec.Process"), IWorkEntityAction
{
    public Task<Package<IEntity?>> Handle(WorkContext context, Package<DoNamedInput<IEntity?>> package)
        => package.Data.Action.ToLowerInvariant() switch
        {
            PreEdit or PreSave => new DataProcessorBlockUserWithoutElevation(user, UserElevation.SiteAdmin, package.Data.Action)
                .Handle(context, package.RePackage(package.Data.Input)),
            _ => Task.FromResult(package.Data.Input.ToPackage())
        };
}



internal class DataProcessorBlockUserWithoutElevation(IUser user, UserElevation elevation, string verb) : IWorkEntity
{
    public async Task<Package<IEntity?>> Handle(WorkContext context, Package<IEntity?> package)
        => user.GetElevation().IsAtLeast(elevation)
            ? package
            : new()
            {
                Data = null,
                Decision = DataPreprocessorDecision.Error,
                Exceptions = [new UnauthorizedAccessException($"User is not authorized to {verb} this entity.")]
            };
}

