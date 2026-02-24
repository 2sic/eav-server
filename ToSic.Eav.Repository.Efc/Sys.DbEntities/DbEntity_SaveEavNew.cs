using ToSic.Eav.Data.Build.Sys;
using ToSic.Eav.Data.Sys.Entities;

namespace ToSic.Eav.Repository.Efc.Sys.DbEntities;

partial class DbEntity
{

    public TsDynDataEntity CreateDbRecord(IEntity newEnt, int transactionId, int contentTypeId)
    {
        var l = LogDetails.Fn<TsDynDataEntity>($"a:{DbStore.AppId}, guid:{newEnt.EntityGuid}, type:{contentTypeId}");
        var dbEnt = new TsDynDataEntity
        {
            AppId = DbStore.AppId,
            TargetTypeId = newEnt.MetadataFor?.TargetType ?? (int)TargetTypes.None,
            KeyNumber = newEnt.MetadataFor?.KeyNumber,
            KeyGuid = newEnt.MetadataFor?.KeyGuid,
            KeyString = newEnt.MetadataFor?.KeyString,
            TransCreatedId = transactionId,
            TransModifiedId = transactionId,
            EntityGuid = newEnt.EntityGuid != Guid.Empty ? newEnt.EntityGuid : Guid.NewGuid(),
            IsPublished = newEnt.IsPublished,
            PublishedEntityId = newEnt.IsPublished ? null : ((Entity)newEnt).GetInternalPublishedIdForSaving(),
            Owner = string.IsNullOrEmpty(newEnt.Owner) ? DbStore.UserIdentityToken : newEnt.Owner,
            ContentTypeId = contentTypeId,
            Version = 1,
            Json = null // use null, as we must wait to serialize till we have the entityId
        };

        // Moved down to better batch
        //DbContext.DoAndSaveWithoutChangeDetection(() => DbContext.SqlDb.Add(dbEnt), "save new");
        return l.ReturnAsOk(dbEnt);
    }

    public void SaveCreatedNoChangeDetection(IEnumerable<TsDynDataEntity> newDbEntities)
        => DbStore.DoAndSaveWithoutChangeDetection(() => DbStore.SqlDb.AddRange(newDbEntities), "save new");
}