using ToSic.Eav.Data.Build.Sys;
using ToSic.Eav.Repository.Efc.Sys.DbEntities;

namespace ToSic.Eav.Repository.Efc.Sys.DbEntityProcess;
internal class Process1Preflight(): Process0Base("DB.EPrc1")
{
    public override EntityProcessData ProcessOne(EntityProcessServices services, EntityProcessData data)
    {
        var newEnt = data.NewEntity;

        var l = services.LogDetails.Fn<EntityProcessData>($"id:{newEnt?.EntityId}/{newEnt?.EntityGuid}, logDetails:{data.LogDetails}");

        if (newEnt == null)
            return l.ReturnAsError(data with { Exception = new ArgumentNullException(nameof(newEnt)) });

        if (newEnt.Type == null! /* paranoid */)
            return l.ReturnAsError(data with { Exception = new("trying to save entity without known content-type, cannot continue") });

        #region Test what languages are given, and check if they exist in the target system

        // continue here - must ensure that the languages are passed in, cached - or are cached on the DbEntity... for multiple saves
        var zoneLangs = data.Languages;

        var usedLanguages = data.NewEntity.GetUsedLanguages();
        if (usedLanguages.Count > 0)
            if (!usedLanguages.All(lang => zoneLangs.Any(zl => zl.Matches(lang.Key))))
            {
                var langList = l.Try(() => string.Join(",", usedLanguages.Select(lang => lang.Key)));
                return l.ReturnAsError(data with
                {
                    Exception = new($"entity has languages missing in zone - entity: {usedLanguages.Count} zone: {zoneLangs.Count} used-list: '{langList}'")
                }
                );
            }

        if (data.LogDetails)
        {
            l.A($"lang checks - zone language⋮{zoneLangs.Count}, usedLanguages⋮{usedLanguages.Count}");
            var zoneLangList = l.Try(() => string.Join(",", zoneLangs.Select(z => z.EnvironmentKey)));
            var usedLangList = l.Try(() => string.Join(",", usedLanguages.Select(u => u.Key)));
            l.A($"langs zone:[{zoneLangList}] used:[{usedLangList}]");
        }


        #endregion Test languages exist

        // check if saving should be with db-type or with the plain json
        data = data with { SaveJson = newEnt.UseJson() };
        if (data.LogDetails)
            l.A($"save json:{data.SaveJson}");

        return l.Return(data);
    }

}
