using ToSic.Eav.Data;
using ToSic.Eav.Data.Processing;
using ToSic.Sys.Users;
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously

namespace ToSic.Eav.Metadata.Sys;

internal class PermissionDataProcessor(IUser user) : ServiceBase("Sec.Process"), IDataProcessor, IDataProcessorPreSave, IDataProcessorPreEdit
{
    async Task<DataProcessorResult<IEntity?>> IDataProcessorPreSave.Process(DataProcessorResult<IEntity?> entity) =>
        await ProcessInternal(entity, "save");

    async Task<DataProcessorResult<IEntity?>> IDataProcessorPreEdit.Process(DataProcessorResult<IEntity?> entity) =>
        await ProcessInternal(entity, "edit");

    private async Task<DataProcessorResult<IEntity?>> ProcessInternal(DataProcessorResult<IEntity?> data, string verb) =>
        user.IsSiteAdmin
            ? data
            : new()
            {
                Data = null,
                Decision = DataPreprocessorDecision.Error,
                Exceptions = [new UnauthorizedAccessException($"User is not authorized to {verb} this entity.")]
            };
}
