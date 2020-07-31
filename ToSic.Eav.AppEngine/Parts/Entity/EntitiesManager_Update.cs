using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Data;
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
        /// <param name="draft">Optionally specify that it should be a draft change</param>
        public void UpdateParts(int id, UpdateList values, bool? draft = null)
        {
            var wrapLog = Log.Call($"id:{id}");
            UpdateParts(AppManager.AppState.List.FindRepoId(id), values, draft);
            wrapLog("ok");
        }

        // 2020-07-31 2dm - never used
        //public void DoAndUpdate(IEntity entity, Func<UpdateList> callback, bool? draft = null)
        //{
        //    var wrapLog = Log.Call();
        //    var values = callback.Invoke();
        //    AppManager.Entities.UpdateParts(entity, values, draft);
        //    wrapLog("ok");
        //}


        /// <summary>
        /// Update an entity
        /// </summary>
        /// <param name="orig">Original entity to be updated</param>
        /// <param name="values">Dictionary of values to update</param>
        /// <param name="draft">Optionally specify that it should be a draft change</param>
        public void UpdateParts(IEntity orig, Dictionary<string, object> values, bool? draft = null)
        {
            var wrapLog = Log.Call();
            if (values == null || !values.Any())
            {
                wrapLog("nothing to save");
                return;
            }

            var saveOptions = SaveOptions.Build(AppManager.ZoneId);
            saveOptions.PreserveUntouchedAttributes = true;
            saveOptions.PreserveUnknownLanguages = true;

            var tempEnt = new Entity(AppManager.AppId, 0, orig.Type, values);
            var saveEnt = new EntitySaver(Log)
                .CreateMergedForSaving(orig, tempEnt, saveOptions);

            // if changes should be draft, ensure it works
            if (draft.HasValue && draft.Value)
            {
                ((Entity)saveEnt).PlaceDraftInBranch = true;
                ((Entity)saveEnt).IsPublished = false;
            }

            Save(saveEnt, saveOptions);
            wrapLog("ok");
        }
    }
}
