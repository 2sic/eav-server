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
            PreEdit or PreSave => new DataProcessorBlockUserWithoutElevation(user)
                .Handle(context, package.RePackage(new DataProcessorBlockUserWithoutElevation.Payload(package.Data) { ExpectedElevation = UserElevation.SiteAdmin })),
            _ => Task.FromResult(package.Data.Input.ToPackage())
        };
}



internal class DataProcessorBlockUserWithoutElevation(IUser user)
{
    public Task<Package<IEntity?>> Handle(WorkContext context, Package<Payload> package)
        => Task.FromResult(user.GetElevation().IsAtLeast(package.Data.ExpectedElevation)
            ? package.RePackage(package.Data.Input)
            : new()
            {
                Data = null,
                Decision = DataPreprocessorDecision.Error,
                Exceptions = [new UnauthorizedAccessException($"User is not authorized to {package.Data.Action} this entity.")]
            });

    public record Payload : DoNamedInput<IEntity?>
    {
        public Payload(DoNamedInput<IEntity?> input) : base(input)
        { }

        public required UserElevation ExpectedElevation { get; init; }

    }
}

