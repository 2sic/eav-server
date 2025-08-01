﻿using ToSic.Sys.Utils;

namespace ToSic.Eav.Apps.Sys.Work;

[ShowApiWhenReleased(ShowApiMode.Never)]
public class WorkContentTypesMod() : WorkUnitBase<IAppWorkCtxWithDb>("ApS.InpGet")
{
    public void Create(string nameId, string scope)
    {
        var l = Log.Fn();
        var ct = AppWorkCtx.DbStorage.ContentTypes.PrepareDbContentType(nameId, nameId, scope, false, AppWorkCtx.AppId);
        if (ct != null)
            AppWorkCtx.DbStorage.DoAndSaveWithoutChangeDetection(() => AppWorkCtx.DbStorage.SqlDb.Add(ct));
        l.Done();
    }

    public bool AddOrUpdate(string staticName, string scope, string name, int? usesConfigurationOfOtherSet,
        bool alwaysShareConfig)
    {
        var l = Log.Fn<bool>($"save {AppWorkCtx.Show()}");
        if (name.IsEmptyOrWs())
            return l.ReturnFalse("name was empty, will cancel");

        AppWorkCtx.DbStorage.ContentType.AddOrUpdate(staticName, scope, name, usesConfigurationOfOtherSet, alwaysShareConfig);
        return l.ReturnTrue();
    }

    public bool CreateGhost(string sourceStaticName)
    {
        var l = Log.Fn<bool>($"create ghost a#{AppWorkCtx.Show()}, type:{sourceStaticName}");
        AppWorkCtx.DbStorage.ContentType.CreateGhost(sourceStaticName);
        return l.ReturnTrue();
    }


    public void SetTitle(int contentTypeId, int attributeId)
    {
        var l = Log.Fn($"set title type#{contentTypeId}, attrib:{attributeId}");
        AppWorkCtx.DbStorage.Attributes.SetTitleAttribute(attributeId, contentTypeId);
        l.Done();
    }

    public bool Delete(string staticName)
    {
        var l = Log.Fn<bool>($"delete a#{AppWorkCtx.Show()}, name:{staticName}");
        AppWorkCtx.DbStorage.ContentType.Delete(staticName);
        return l.ReturnTrue();
    }

}