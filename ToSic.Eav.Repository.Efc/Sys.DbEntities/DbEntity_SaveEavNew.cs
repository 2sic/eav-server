﻿
using ToSic.Eav.Data.Sys.Entities;

namespace ToSic.Eav.Repository.Efc.Sys.DbEntities;

partial class DbEntity
{

    public TsDynDataEntity CreateDbRecord(IEntity newEnt, int transactionId, int contentTypeId)
    {
        var l = LogDetails.Fn<TsDynDataEntity>($"a:{DbContext.AppId}, guid:{newEnt.EntityGuid}, type:{contentTypeId}");
        var dbEnt = new TsDynDataEntity
        {
            AppId = DbContext.AppId,
            TargetTypeId = newEnt.MetadataFor?.TargetType ?? (int)TargetTypes.None,
            KeyNumber = newEnt.MetadataFor?.KeyNumber,
            KeyGuid = newEnt.MetadataFor?.KeyGuid,
            KeyString = newEnt.MetadataFor?.KeyString,
            TransCreatedId = transactionId,
            TransModifiedId = transactionId,
            EntityGuid = newEnt.EntityGuid != Guid.Empty ? newEnt.EntityGuid : Guid.NewGuid(),
            IsPublished = newEnt.IsPublished,
            PublishedEntityId = newEnt.IsPublished ? null : ((Entity)newEnt).GetInternalPublishedIdForSaving(),
            Owner = string.IsNullOrEmpty(newEnt.Owner) ? DbContext.UserIdentityToken : newEnt.Owner,
            ContentTypeId = contentTypeId,
            Version = 1,
            Json = null // use null, as we must wait to serialize till we have the entityId
        };

        DbContext.DoAndSaveWithoutChangeDetection(() => DbContext.SqlDb.Add(dbEnt), "save new");
        return l.ReturnAsOk(dbEnt);
    }
}