using ToSic.Eav.Data;
using ToSic.Eav.Data.Processing;
using ToSic.Sys.Users;
using static ToSic.Eav.Data.Processing.DataProcessingEvents;

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously

namespace ToSic.Eav.Metadata.Sys;

internal class PermissionDataProcessor(IUser user) : ServiceBase("Sec.Process"), IDataProcessor
{
    public Task<DataProcessorResult<IEntity?>> Process(string action, DataProcessorResult<IEntity?> entity) =>
        action.ToLowerInvariant() switch
        {
            PreEdit or PreSave => new DataProcessorBlockUserWithoutElevation(user, UserElevation.SiteAdmin, action).Process(entity),
            _ => Task.FromResult(entity)
        };
}



internal class DataProcessorBlockUserWithoutElevation(IUser user, UserElevation elevation, string verb) : IDataProcessorAction
{
    public async Task<DataProcessorResult<IEntity?>> Process(DataProcessorResult<IEntity?> data) =>
        user.GetElevation().IsAtLeast(elevation)
            ? data
            : new()
            {
                Data = null,
                Decision = DataPreprocessorDecision.Error,
                Exceptions = [new UnauthorizedAccessException($"User is not authorized to {verb} this entity.")]
            };
}

