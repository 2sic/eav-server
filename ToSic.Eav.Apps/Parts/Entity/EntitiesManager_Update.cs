using System.Linq;
using ToSic.Eav.Data;
using ToSic.Eav.Logging;
using ToSic.Eav.Persistence;
using UpdateList = System.Collections.Generic.Dictionary<string, object>;

namespace ToSic.Eav.Apps.Parts
{
    public partial class EntitiesManager
    {
        /// <summary>
        /// Update an entity
        /// </summary>
        /// <param name="id"></param>
        /// <param name="values"></param>
        /// <param name="draftAndBranch">Optionally specify that it should be a draft change</param>
        public void UpdateParts(int id, UpdateList values, (bool published, bool branch)? draftAndBranch = null)
        {
            var wrapLog = Log.Fn($"id:{id}");
            UpdatePartsFromValues(Parent.AppState.List.FindRepoId(id), values, draftAndBranch);
            wrapLog.Done("ok");
        }

        /// <summary>
        /// Update an entity
        /// </summary>
        /// <param name="id"></param>
        /// <param name="values"></param>
        /// <param name="draftAndBranch">Optionally specify that it should be a draft change</param>
        public void UpdateParts(int id, Entity values, (bool published, bool branch)? draftAndBranch = null)
        {
            var wrapLog = Log.Fn($"id:{id}");
            UpdatePartFromEntity(Parent.AppState.List.FindRepoId(id), values, draftAndBranch);
            wrapLog.Done("ok");
        }

        /// <summary>
        /// Update an entity
        /// </summary>
        /// <param name="orig">Original entity to be updated</param>
        /// <param name="values">Dictionary of values to update</param>
        /// <param name="draftAndBranch">Optionally specify that it should be a draft change</param>
        private bool UpdatePartsFromValues(IEntity orig, UpdateList values, (bool published, bool branch)? draftAndBranch = null)
        {
            var wrapLog = Log.Call<bool>();
            var tempEnt = CreatePartialEntityOld(orig, values);
            if (tempEnt == null) return wrapLog("nothing to import", false);
            var result = UpdatePartFromEntity(orig, tempEnt, draftAndBranch);
            return wrapLog($"{result}", true);
        }

        /// <summary>
        /// Update an entity
        /// </summary>
        /// <param name="orig">Original entity to be updated</param>
        /// <param name="partialEntity">Partial Entity to update</param>
        /// <param name="draftAndBranch">Optionally specify that it should be a draft change</param>
        private bool UpdatePartFromEntity(IEntity orig, Entity partialEntity, (bool published, bool branch)? draftAndBranch = null)
        {
            var wrapLog = Log.Call<bool>();
            if (partialEntity == null)
                return wrapLog("nothing to import", false);

            var saveOptions = Environment.SaveOptions(Parent.ZoneId);
            saveOptions.PreserveUntouchedAttributes = true;
            saveOptions.PreserveUnknownLanguages = true;

            var saveEnt = _entitySaverLazy.Ready.CreateMergedForSaving(orig, partialEntity, saveOptions);

            // if changes should be draft, ensure it works
            if (draftAndBranch.HasValue)
            {
                saveEnt.IsPublished = draftAndBranch.Value.published;
                saveEnt.PlaceDraftInBranch = draftAndBranch.Value.branch;
            }

            Save(saveEnt, saveOptions);
            return wrapLog("ok", true);
        }

        private Entity CreatePartialEntityOld(IEntity orig, UpdateList values)
        {
            var wrapLog = Log.Call<Entity>();
            if (values == null || !values.Any())
                return wrapLog("nothing to save", null);

            return wrapLog("ok", new Entity(Parent.AppId, 0, orig.Type, values));
        }
    }
}
