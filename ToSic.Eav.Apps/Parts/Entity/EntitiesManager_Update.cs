using System.Linq;
using ToSic.Eav.Data;
using ToSic.Lib.Logging;
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
        public void UpdateParts(int id, UpdateList values, (bool published, bool branch)? draftAndBranch = null) => 
            Log.Do($"id:{id}", () => UpdatePartsFromValues(Parent.AppState.List.FindRepoId(id), values, draftAndBranch));

        /// <summary>
        /// Update an entity
        /// </summary>
        /// <param name="id"></param>
        /// <param name="values"></param>
        /// <param name="draftAndBranch">Optionally specify that it should be a draft change</param>
        public void UpdateParts(int id, Entity values, (bool published, bool branch)? draftAndBranch = null) =>
            Log.Do($"id:{id}", () => UpdatePartFromEntity(Parent.AppState.List.FindRepoId(id), values, draftAndBranch));

        /// <summary>
        /// Update an entity
        /// </summary>
        /// <param name="orig">Original entity to be updated</param>
        /// <param name="values">Dictionary of values to update</param>
        /// <param name="draftAndBranch">Optionally specify that it should be a draft change</param>
        private bool UpdatePartsFromValues(IEntity orig, UpdateList values, (bool published, bool branch)? draftAndBranch = null) => Log.Func(() =>
        {
            var tempEnt = CreatePartialEntityOld(orig, values);
            if (tempEnt == null) return (false, "nothing to import");
            var result = UpdatePartFromEntity(orig, tempEnt, draftAndBranch);
            return (true, $"{result}");
        });

        /// <summary>
        /// Update an entity
        /// </summary>
        /// <param name="orig">Original entity to be updated</param>
        /// <param name="partialEntity">Partial Entity to update</param>
        /// <param name="draftAndBranch">Optionally specify that it should be a draft change</param>
        private bool UpdatePartFromEntity(IEntity orig, Entity partialEntity, (bool published, bool branch)? draftAndBranch = null) => Log.Func(() =>
        {
            if (partialEntity == null)
                return (false, "nothing to import");

            var saveOptions = _environmentLazy.Value.SaveOptions(Parent.ZoneId);
            saveOptions.PreserveUntouchedAttributes = true;
            saveOptions.PreserveUnknownLanguages = true;

            var saveEnt = _entitySaverLazy.Value.CreateMergedForSaving(orig, partialEntity, saveOptions);

            // if changes should be draft, ensure it works
            if (draftAndBranch.HasValue)
            {
                saveEnt.IsPublished = draftAndBranch.Value.published;
                saveEnt.PlaceDraftInBranch = draftAndBranch.Value.branch;
            }

            Save(saveEnt, saveOptions);
            return (true, "ok");
        });

        private Entity CreatePartialEntityOld(IEntity orig, UpdateList values) => Log.Func(() =>
        {
            if (values == null || !values.Any())
                return (null, "nothing to save");

            return (_multiBuilder.Value.Entity.Create(appId: Parent.AppId, contentType: orig.Type, rawValues: values), "ok");
        });
    }
}
