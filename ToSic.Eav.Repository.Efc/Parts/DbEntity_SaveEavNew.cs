using System;
using ToSic.Eav.Data;
using ToSic.Eav.Data.Builder;
using ToSic.Lib.Logging;
using ToSic.Eav.Metadata;
using ToSic.Eav.Persistence.Efc.Models;

namespace ToSic.Eav.Repository.Efc.Parts
{
    public partial class DbEntity
    {

        private ToSicEavEntities CreateDbRecord(IEntity newEnt, int changeId, int contentTypeId)
        {
            var wrapLog = Log.Fn<ToSicEavEntities>($"a:{DbContext.AppId}, guid:{newEnt.EntityGuid}, type:{contentTypeId}");
            var dbEnt = new ToSicEavEntities
            {
                AppId = DbContext.AppId,
                AssignmentObjectTypeId = newEnt.MetadataFor?.TargetType ?? (int)TargetTypes.None,
                KeyNumber = newEnt.MetadataFor?.KeyNumber,
                KeyGuid = newEnt.MetadataFor?.KeyGuid,
                KeyString = newEnt.MetadataFor?.KeyString,
                ChangeLogCreated = changeId,
                ChangeLogModified = changeId,
                EntityGuid = newEnt.EntityGuid != Guid.Empty ? newEnt.EntityGuid : Guid.NewGuid(),
                IsPublished = newEnt.IsPublished,
                PublishedEntityId = newEnt.IsPublished ? null : ((Entity)newEnt).GetPublishedIdForSaving(),
                Owner = string.IsNullOrEmpty(newEnt.Owner) ? DbContext.UserName : newEnt.Owner,
                AttributeSetId = contentTypeId,
                Version = 1,
                Json = null // use null, as we must wait to serialize till we have the entityId
            };

            DbContext.DoAndSaveWithoutChangeDetection(() => DbContext.SqlDb.Add(dbEnt), "save new");
            return wrapLog.ReturnAsOk(dbEnt);
        }
    }
}
