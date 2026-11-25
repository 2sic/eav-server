using ToSic.Sys.HookUp;

namespace ToSic.Eav.Ui.Edit.Sys.Load.Prefetch;

[ShowApiWhenReleased(ShowApiMode.Never)]
public class EditLoadActivityPrefetchEntity(EditPrefetchEntitiesSvc entityPickerBackend)
    : ServiceBase("Edt.PreEnt", connect: [entityPickerBackend]),
        IEditLoadPrefetchProvider
{
    public const string NameId = "Entities";

    public Task<Package<object>> Handle(WorkContext mainCtx, Package<EditLoadDto> package)
    {
        var result = PrefetchEntities(mainCtx.Get<int>(EditLoadContextConstants.AppId), package.Data);
        return Task.FromResult(package.RePackage(result as object));
    }

    public ICollection<EntityForPickerDto> PrefetchEntities(int appId, EditLoadDto editData)
    {
        var l = Log.Fn<ICollection<EntityForPickerDto>>();
        try
        {
            // Step 1: try to find entity fields
            var bundlesHavingEntities = editData.Items
                // Only these with entity fields
                .Where(b => b.Entity?.Attributes?.Entity?.Any() ?? false)
                .Select(b => new
                {
                    b.Entity!.Guid,
                    b.Entity.Attributes.Entity
                })
                .ToListOpt();

            var entities = bundlesHavingEntities
                .SelectMany(set => set.Entity!
                    .SelectMany(e => e.Value
                            ?.SelectMany(entityAttrib => entityAttrib.Value) ?? []
                    )
                )
                .Where(guid => guid != null)
                .Select(guid => guid.ToString()!)
                // Step 2: Check which ones have a link reference
                .ToListOpt();

            // stop here if nothing found, otherwise the backend will return all entities
            if (!entities.Any())
                return l.Return([], "none found");

            var items = entityPickerBackend.GetForEntityPicker(appId, entities, null, allowFromAllScopes: true);
            return l.Return(items, $"{items.Count}");
        }
        catch
        {
            return l.Return([new() { Id = -1, Text = "Error occurred pre-fetching entities", Value = Guid.Empty }], "error");
        }
    }
}
