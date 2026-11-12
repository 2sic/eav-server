using ToSic.Eav.Apps.Sys.Caching;
using ToSic.Eav.Apps.Sys.Work;
using ToSic.Eav.DataSource.Query.Sys;

namespace ToSic.Eav.DataSources.Query.Sys;

[ShowApiWhenReleased(ShowApiMode.Never)]
public class WorkQueryDelete(AppCachePurger systemManagerLazy, QueryDefinitionService queryDefSvc, AppWorkChain<WorkEntityDelete> delete)
    : ServiceWithSetup<IAppWorkContext>("AWk.QryMod", connect: [systemManagerLazy, queryDefSvc, delete])
{
    public bool Delete(int id)
    {
        var l = Log.Fn<bool>($"delete a#{MyOptions.AppId}, id:{id}");

        var entDelete = delete.New(MyOptions);

        var canDeleteResult = entDelete.CheckForDeletePreWarnings(id);
        if (canDeleteResult.HasMessages)
            throw l.Done(new Exception(canDeleteResult.Messages));


        // Get the Entity describing the Query and Query Parts (DataSources)
        var qDef = queryDefSvc.GetDefinition(MyOptions.AppReader, id);

        var parts = qDef.Parts;
        var mdItems = parts
            .Select(ds => (ds as ICanBeEntity).Entity.Metadata.FirstOrDefault())
            .Where(md => md != null)
            .Select(md => md!.EntityId)
            .ToListOpt();

        // delete in the right order - first the outermost-dependents, then a layer in, and finally the top node
        entDelete.Delete(mdItems);
        entDelete.Delete(parts.Select(p => p.Id).ToListOpt());
        entDelete.Delete(id);

        // flush cache
        systemManagerLazy.PurgeApp(MyOptions.AppId);

        return l.ReturnTrue();
    }
}
