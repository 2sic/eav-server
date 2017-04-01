using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using ToSic.Eav.BLL;
using ToSic.Eav.BLL.Parts;
using ToSic.Eav.ImportExport;

namespace ToSic.Eav.Persistence
{
    internal class DbVersioning: BllCommandBase
    {
        internal DbVersioning(EavDataController cntx) : base(cntx)
        {
        }

        internal int MainChangeLogId;


        /// <summary>
        /// Creates a ChangeLog immediately
        /// </summary>
        /// <remarks>Also opens the SQL Connection to ensure this ChangeLog is used for Auditing on this SQL Connection</remarks>
        internal int GetChangeLogId(string userName)
        {
            if (MainChangeLogId == 0)
            {
                if (Context.SqlDb.Connection.State != ConnectionState.Open)
                    Context.SqlDb.Connection.Open();	// make sure same connection is used later
                MainChangeLogId = Context.SqlDb.AddChangeLog(userName).Single().ChangeID;
            }

            return MainChangeLogId;
        }

        /// <summary>
        /// Creates a ChangeLog immediately
        /// </summary>
        internal int GetChangeLogId()
        {
            return GetChangeLogId(Context.UserName);
        }

        /// <summary>
        /// Set ChangeLog ID on current Context and connection
        /// </summary>
        /// <param name="changeLogId"></param>
        public void SetChangeLogId(int changeLogId)
        {
            if (MainChangeLogId != 0)
                throw new Exception("ChangeLogID was already set");


            Context.SqlDb.Connection.Open();	// make sure same connection is used later
            Context.SqlDb.SetChangeLogIdInternal(changeLogId);
            MainChangeLogId = changeLogId;
        }




        /// <summary>
        /// Persist modified Entity to DataTimeline
        /// </summary>
        internal void SaveEntityToDataTimeline(Entity currentEntity)
        {
            var export = new XmlExport(Context);
            var entityModelSerialized = export.GetEntityXElementUncached(currentEntity.EntityID);
            var timelineItem = new DataTimelineItem
            {
                SourceTable = "ToSIC_EAV_Entities",
                Operation = Constants.DataTimelineEntityStateOperation,
                NewData = entityModelSerialized.ToString(),
                SourceGuid = currentEntity.EntityGUID,
                SourceID = currentEntity.EntityID,
                SysLogID = GetChangeLogId(),
                SysCreatedDate = DateTime.Now
            };
            Context.SqlDb.AddToDataTimeline(timelineItem);

            Context.SqlDb.SaveChanges();
        }

        /// <summary>
        /// Get an Entity in the specified Version from DataTimeline using XmlImport
        /// </summary>
        /// <param name="entityId">EntityId</param>
        /// <param name="changeId">ChangeId to retrieve</param>
        /// <param name="defaultCultureDimension">Default Language</param>
        public Import.ImportEntity GetEntityVersion(int entityId, int changeId, int? defaultCultureDimension)
        {
            // Get Timeline Item
            string timelineItem;
            try
            {
                timelineItem = Context.SqlDb.DataTimeline.Where(d => d.Operation == Constants.DataTimelineEntityStateOperation && d.SourceID == entityId && d.SysLogID == changeId).Select(d => d.NewData).SingleOrDefault();
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
            var assignmentObjectTypeName = xEntity.Attribute("AssignmentObjectType").Value;
            var assignmentObjectTypeId = new DbShortcuts(Context).GetAssignmentObjectType(assignmentObjectTypeName).AssignmentObjectTypeID;

            // Prepare source and target-Languages
            if (!defaultCultureDimension.HasValue)
                throw new NotSupportedException("GetEntityVersion without defaultCultureDimension is not yet supported.");

            var defaultLanguage = Context.Dimensions.GetDimension(defaultCultureDimension.Value).ExternalKey;
            var targetDimensions = Context.Dimensions.GetLanguages();
            var allSourceDimensionIds = ((IEnumerable<object>)xEntity.XPathEvaluate("/Value/Dimension/@DimensionID")).Select(d => int.Parse(((XAttribute)d).Value)).ToArray();
            var allSourceDimensionIdsDistinct = allSourceDimensionIds.Distinct().ToArray();
            var sourceDimensions = Context.Dimensions.GetDimensions(allSourceDimensionIdsDistinct).ToList();
            int sourceDefaultDimensionId;
            if (allSourceDimensionIdsDistinct.Contains(defaultCultureDimension.Value))	// if default culture exists in the Entity, sourceDefaultDimensionId is still the same
                sourceDefaultDimensionId = defaultCultureDimension.Value;
            else
            {
                var sourceDimensionsIdsGrouped = (from n in allSourceDimensionIds group n by n into g select new { DimensionId = g.Key, Qty = g.Count() }).ToArray();
                sourceDefaultDimensionId = sourceDimensionsIdsGrouped.Any() ? sourceDimensionsIdsGrouped.OrderByDescending(g => g.Qty).First().DimensionId : defaultCultureDimension.Value;
            }

            // Load Entity from Xml unsing XmlImport
            return XmlImport.GetImportEntity(xEntity, assignmentObjectTypeId, targetDimensions, sourceDimensions, sourceDefaultDimensionId, defaultLanguage);
        }

        /// <summary>
        /// Get all Versions of specified EntityId
        /// </summary>
        public DataTable GetEntityVersions(int entityId)
        {
            var entityVersions = GetEntityHistory(entityId);

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

        public List<EntityHistoryItem> GetEntityHistory(int entityId)
        {
            // get Versions from DataTimeline
            var entityVersions = (from d in Context.SqlDb.DataTimeline
                join c in Context.SqlDb.ChangeLogs on d.SysLogID equals c.ChangeID
                where d.Operation == Constants.DataTimelineEntityStateOperation && d.SourceID == entityId
                orderby c.Timestamp descending
                select new EntityHistoryItem() { SysCreatedDate = d.SysCreatedDate, User = c.User, ChangeId = c.ChangeID}).ToList();
            return entityVersions;
        }

        public class EntityHistoryItem
        {
            public DateTime SysCreatedDate { get; set; }
            public string User { get; set; }
            public int ChangeId { get; set; }

        }


        /// <summary>
        /// Get the Values of an Entity in the specified Version
        /// </summary>
        public DataTable GetEntityVersionValues(int entityId, int changeId, int? defaultCultureDimension, string multiValuesSeparator = null)
        {
            var entityVersion = GetEntityVersion(entityId, changeId, defaultCultureDimension);

            var result = new DataTable();
            result.Columns.Add("Field");
            result.Columns.Add("Language");
            result.Columns.Add("Value");
            result.Columns.Add("SharedWith");

            foreach (var attribute in entityVersion.Values)
            {
                foreach (var valueModel in attribute.Value)
                {
                    var firstLanguage = valueModel.ValueDimensions.First().DimensionExternalKey;
                    result.Rows.Add(attribute.Key, firstLanguage, Context.Values.GetTypedValue(valueModel, multiValuesSeparator: multiValuesSeparator));	// Add Main-Language

                    foreach (var valueDimension in valueModel.ValueDimensions.Skip(1))	// Add additional Languages
                    {
                        result.Rows.Add(attribute.Key, valueDimension.DimensionExternalKey, Context.Values.GetTypedValue(valueModel, multiValuesSeparator: multiValuesSeparator), firstLanguage + (valueDimension.ReadOnly ? " (read)" : " (write)"));
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Restore an Entity to the specified Version by creating a new Version using the Import
        /// </summary>
        public void RestoreEntityVersion(int entityId, int changeId, int? defaultCultureDimension)
        {
            // Get Entity in specified Version/ChangeId
            var newVersion = GetEntityVersion(entityId, changeId, defaultCultureDimension);

            // Restore Entity
            var import = new Import.Import(Context.ZoneId /* _zoneId*/,Context.AppId /* _appId*/, Context.UserName, false, false);
            import.RunImport(null, new List<Import.ImportEntity> { newVersion });

            // Delete Draft (if any)
            var entityDraft = new DbLoadIntoEavDataStructure(Context).GetEavEntity(entityId).GetDraft();
            if (entityDraft != null)
                Context.Entities.DeleteEntity(entityDraft.RepositoryId);
        }

    }
}
