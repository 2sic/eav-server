using System;
using System.Collections.Generic;
using System.Xml.Linq;
using ToSic.Eav.Apps.ImportExport;
using ToSic.Eav.Data;
using ToSic.Eav.ImportExport.Versioning;
using ToSic.Eav.ImportExport.Xml;
using ToSic.Eav.Persistence.Interfaces;
using ToSic.Eav.Repository.Efc.Parts;

namespace ToSic.Eav.Apps.Parts
{
    public partial class EntitiesManager
    {

        public List<ItemHistory> VersionHistory(int id, bool includeData = true) => _appManager.DataController.Versioning.GetHistoryList(id, includeData);

        /// <summary>
        /// Restore an Entity to the specified Version by creating a new Version using the Import
        /// </summary>
        public void VersionRestore(int entityId, int changeId)
        {
            // Get Entity in specified Version/ChangeId
            var newVersion = PrepareRestoreEntity(entityId, changeId);

            // Restore Entity
            var import = new Import(_appManager.ZoneId, _appManager.AppId, false, false);
            import.ImportIntoDb(null, new List<Entity> { newVersion });

            // Delete Draft (if any)
            var entityDraft = _appManager.DataController.Publishing.GetDraftBranchEntityId(entityId);
            if (entityDraft.HasValue)
                _appManager.DataController.Entities.DeleteEntity(entityDraft.Value);

            SystemManager.Purge(_appManager.ZoneId, _appManager.AppId);
        }


        /// <summary>
        /// Get an Entity in the specified Version from DataTimeline using XmlImport
        /// </summary>
        /// <param name="entityId">EntityId</param>
        /// <param name="changeId">ChangeId to retrieve</param>
        ///// <param name="defaultCultureDimension">Default Language</param>
        private Entity PrepareRestoreEntity(int entityId, int changeId)
        {
            var environment = Factory.Resolve<IImportExportEnvironment>();
            var defLanguage = environment.DefaultLanguage;

            var xEntity = GetTimelineItemOrThrowError(entityId, changeId);

            #region language detection / assignment temporarily not working yet - going without languages first
            var envLanguages = new ZoneRuntime(_appManager.ZoneId).Languages(true);
            var srcLanguages = new ZoneRuntime(_appManager.ZoneId).Languages(true);//_appManager.DataController.Dimensions.GetLanguages(true);

            #endregion

            // Load Entity from Xml unsing XmlImport
            var xmlBuilder = new XmlToImportEntity(srcLanguages, null, envLanguages, defLanguage);
            return xmlBuilder.BuildEntityFromXml(xEntity, /*envLanguages, srcLanguages, null, defLanguage,*/ null);
        }

        private XElement GetTimelineItemOrThrowError(int entityId, int changeId)
        {
            // Get Timeline Item
            string timelineItem;
            try
            {
                timelineItem = _appManager.DataController.Versioning.GetItem(entityId, changeId).Data;
            }
            catch (InvalidOperationException ex)
            {
                throw new InvalidOperationException(
                    $"Error getting EntityId {entityId} with ChangeId {changeId} from DataTimeline. {ex.Message}");
            }
            if (timelineItem == null)
                throw new InvalidOperationException(
                    $"EntityId {entityId} with ChangeId {changeId} not found in DataTimeline.");

            // Parse XML
            try
            {
                var xEntity = XElement.Parse(timelineItem);
                return xEntity;
            }
            catch (Exception ex)
            {
                throw new Exception("trying to parse history-xml of this entity, but failed", ex);
            }
        }

    }
}
