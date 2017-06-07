using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using ToSic.Eav.ImportExport;
using ToSic.Eav.ImportExport.Models;
using ToSic.Eav.ImportExport.Xml;
using ToSic.Eav.Persistence.Efc;

namespace ToSic.Eav.Repository.Efc.Parts
{
    internal partial class DbVersioning
    {

        #region Currently still unused versioning stuff!!!

        /// <summary>
        /// Get an Entity in the specified Version from DataTimeline using XmlImport
        /// </summary>
        /// <param name="entityId">EntityId</param>
        /// <param name="changeId">ChangeId to retrieve</param>
        /// <param name="defaultCultureDimension">Default Language</param>
        private ImpEntity Unused_GetEntityVersion(int entityId, int changeId, int? defaultCultureDimension)
        {
            // Get Timeline Item
            string timelineItem;
            try
            {
                timelineItem = DbContext.SqlDb.ToSicEavDataTimeline.Where(d => d.Operation == Constants.DataTimelineEntityStateOperation && d.SourceId == entityId && d.SysLogId == changeId).Select(d => d.NewData).SingleOrDefault();
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
            var xEntity = XElement.Parse(timelineItem);
            var assignmentObjectTypeName = xEntity.Attribute(XmlConstants.KeyTargetType /*"AssignmentObjectType" */).Value;
            var assignmentObjectTypeId = new DbShortcuts(DbContext).GetAssignmentObjectType(assignmentObjectTypeName).AssignmentObjectTypeId;

            // Prepare source and target-Languages
            if (!defaultCultureDimension.HasValue)
                throw new NotSupportedException("GetEntityVersion without defaultCultureDimension is not yet supported.");

            var defaultLanguage = DbContext.Dimensions.GetDimension(defaultCultureDimension.Value).ExternalKey;
            var targetDimensions = DbContext.Dimensions.GetLanguages();
            var allSourceDimensionIds = ((IEnumerable<object>)xEntity.XPathEvaluate("/Value/Dimension/@DimensionId")).Select(d => int.Parse(((XAttribute)d).Value)).ToArray();
            var allSourceDimensionIdsDistinct = allSourceDimensionIds.Distinct().ToArray();
            var sourceDimensions = DbContext.Dimensions.GetDimensions(allSourceDimensionIdsDistinct).ToList();
            int sourceDefaultDimensionId;
            if (allSourceDimensionIdsDistinct.Contains(defaultCultureDimension.Value))	// if default culture exists in the Entity, sourceDefaultDimensionId is still the same
                sourceDefaultDimensionId = defaultCultureDimension.Value;
            else
            {
                var sourceDimensionsIdsGrouped = (from n in allSourceDimensionIds group n by n into g select new { DimensionId = g.Key, Qty = g.Count() }).ToArray();
                sourceDefaultDimensionId = sourceDimensionsIdsGrouped.Any() ? sourceDimensionsIdsGrouped.OrderByDescending(g => g.Qty).First().DimensionId : defaultCultureDimension.Value;
            }
            var targetDimsRetyped = targetDimensions.Select(d => new Data.Dimension { DimensionId = d.DimensionId, Key = d.ExternalKey}).ToList();
            var sourceDimsRetyped = sourceDimensions.Select(s => new Data.Dimension {DimensionId = s.DimensionId, Key = s.ExternalKey}).ToList();
            // Load Entity from Xml unsing XmlImport
            return XmlToImportEntity.BuildImpEntityFromXml(xEntity, assignmentObjectTypeId, targetDimsRetyped, sourceDimsRetyped, sourceDefaultDimensionId, defaultLanguage);
        }

        /// <summary>
        /// Get all Versions of specified EntityId
        /// </summary>
        public DataTable Unused_GetEntityVersions(int entityId)
        {
            var entityVersions = Unused_GetEntityHistory(entityId);

            // Generate DataTable with Version-Numbers
            var versionNumber = entityVersions.Count;	// add version number decrement to prevent additional sorting
            var result = new DataTable();
            result.Columns.Add("Timestamp", typeof(DateTime));
            result.Columns.Add("User", typeof(string));
            result.Columns.Add("ChangeId", typeof(int));
            result.Columns.Add("VersionNumber", typeof(int));
            foreach (var version in entityVersions)
                result.Rows.Add(version.SysCreatedDate, version.User, version.ChangeId, versionNumber--);	// decrement versionnumber

            return result;
        }

        private List<EntityHistoryItem> Unused_GetEntityHistory(int entityId)
        {
            // get Versions from DataTimeline
            var entityVersions = (from d in DbContext.SqlDb.ToSicEavDataTimeline
                join c in DbContext.SqlDb.ToSicEavChangeLog on d.SysLogId equals c.ChangeId
                where d.Operation == Constants.DataTimelineEntityStateOperation && d.SourceId == entityId
                orderby c.Timestamp descending
                select new EntityHistoryItem() { SysCreatedDate = d.SysCreatedDate, User = c.User, ChangeId = c.ChangeId}).ToList();
            return entityVersions;
        }

        private class EntityHistoryItem
        {
            public DateTime SysCreatedDate { get; set; }
            public string User { get; set; }
            public int ChangeId { get; set; }

        }


        /// <summary>
        /// Get the Values of an Entity in the specified Version
        /// </summary>
        public DataTable Unused_GetEntityVersionValues(int entityId, int changeId, int? defaultCultureDimension, string multiValuesSeparator = null)
        {
            var entityVersion = Unused_GetEntityVersion(entityId, changeId, defaultCultureDimension);

            var result = new DataTable();
            result.Columns.Add("Field");
            result.Columns.Add(XmlConstants.EntityLanguage);
            result.Columns.Add(XmlConstants.ValueNode);
            result.Columns.Add("SharedWith");

            foreach (var attribute in entityVersion.Values)
            {
                foreach (var valueModel in attribute.Value)
                {
                    var firstLanguage = valueModel.ValueDimensions.First().DimensionExternalKey;
                    result.Rows.Add(attribute.Key, firstLanguage, DbContext.Values.GetTypedValue(valueModel, multiValuesSeparator: multiValuesSeparator));	// Add Main-Language

                    foreach (var valueDimension in valueModel.ValueDimensions.Skip(1))	// Add additional Languages
                    {
                        result.Rows.Add(attribute.Key, valueDimension.DimensionExternalKey, DbContext.Values.GetTypedValue(valueModel, multiValuesSeparator: multiValuesSeparator), firstLanguage + (valueDimension.ReadOnly ? " (read)" : " (write)"));
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Restore an Entity to the specified Version by creating a new Version using the Import
        /// </summary>
        public void Unused_RestoreEntityVersion(int entityId, int changeId, int? defaultCultureDimension)
        {
            // Get Entity in specified Version/ChangeId
            var newVersion = Unused_GetEntityVersion(entityId, changeId, defaultCultureDimension);

            // Restore Entity
            var import = new DbImport(DbContext.ZoneId /* _zoneId*/, DbContext.AppId /* _appId*/, /*Context.UserName,*/ false, false);
            import.ImportIntoDb(null, new List<ImpEntity> { newVersion });
            
            // IMPORTANT : IF THIS IS EVER USED, REMEMBER TO CLEAR THE CACHE afterwards in the calling method

            // Delete Draft (if any)
            var entityDraft = new Efc11Loader(DbContext.SqlDb).Entity(DbContext.AppId, entityId).GetDraft();
            if (entityDraft != null)
                DbContext.Entities.DeleteEntity(entityDraft.RepositoryId);
        }

        #endregion
    }
}
