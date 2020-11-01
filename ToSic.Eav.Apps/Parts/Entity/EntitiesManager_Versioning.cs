using System;
using System.Collections.Generic;
using ToSic.Eav.Apps.ImportExport;
using ToSic.Eav.Data;
using ToSic.Eav.Persistence.Versions;
using IEntity = ToSic.Eav.Data.IEntity;
using JsonSerializer = ToSic.Eav.ImportExport.Json.JsonSerializer;

namespace ToSic.Eav.Apps.Parts
{
    public partial class EntitiesManager
    {

        public List<ItemHistory> VersionHistory(int id, bool includeData = true) => Parent.DataController.Versioning.GetHistoryList(id, includeData);

        /// <summary>
        /// Restore an Entity to the specified Version by creating a new Version using the Import
        /// </summary>
        public void VersionRestore(int entityId, int changeId)
        {
            // Get Entity in specified Version/ChangeId
            var newVersion = PrepareRestoreEntity(entityId, changeId);

            // Restore Entity
            var import = new Import(Parent.ZoneId, Parent.AppId, false, false);
            import.ImportIntoDb(null, new List<Entity> { newVersion as Entity });

            // Delete Draft (if any)
            var entityDraft = Parent.DataController.Publishing.GetDraftBranchEntityId(entityId);
            if (entityDraft.HasValue)
                Parent.DataController.Entities.DeleteEntity(entityDraft.Value);

            SystemManager.Purge(Parent.ZoneId, Parent.AppId, log: Log); 
        }


        /// <summary>
        /// Get an Entity in the specified Version from DataTimeline using XmlImport
        /// </summary>
        /// <param name="entityId">EntityId</param>
        /// <param name="changeId">ChangeId to retrieve</param>
        ///// <param name="defaultCultureDimension">Default Language</param>
        private IEntity PrepareRestoreEntity(int entityId, int changeId)
        {
            var deserializer = new JsonSerializer(Parent.AppState, Log);

            var str = GetFromTimelime(entityId, changeId);
            return deserializer.Deserialize(str);

        }

        private string GetFromTimelime(int entityId, int changeId)
        {
            try
            {
                var timelineItem = Parent.DataController.Versioning.GetItem(entityId, changeId).Json;
                if (timelineItem != null) return timelineItem;
                throw new InvalidOperationException(
                    $"EntityId {entityId} with ChangeId {changeId} not found in DataTimeline.");
            }
            catch (InvalidOperationException ex)
            {
                throw new InvalidOperationException(
                    $"Error getting EntityId {entityId} with ChangeId {changeId} from DataTimeline. {ex.Message}");
            }
        }
    }
}
