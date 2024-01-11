using ToSic.Eav.Plumbing;
using ToSic.Lib.Logging;

namespace ToSic.Eav.Apps.Work;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class WorkContentTypesMod() : WorkUnitBase<IAppWorkCtxWithDb>("ApS.InpGet")
{
    public void Create(string nameId, string scope)
    {
        var l = Log.Fn();
        AppWorkCtx.DataController.DoAndSave(() =>
            AppWorkCtx.DataController.AttribSet.PrepareDbAttribSet(nameId, nameId, scope, false, AppWorkCtx.AppId));
        l.Done();
    }

    public bool AddOrUpdate(string staticName, string scope, string name, int? usesConfigurationOfOtherSet,
        bool alwaysShareConfig)
    {
        var l = Log.Fn<bool>($"save {AppWorkCtx.Show()}");
        if (name.IsEmptyOrWs())
            return l.ReturnFalse("name was empty, will cancel");

        AppWorkCtx.DataController.ContentType.AddOrUpdate(staticName, scope, name, usesConfigurationOfOtherSet, alwaysShareConfig);
        return l.ReturnTrue();
    }

    public bool CreateGhost(string sourceStaticName)
    {
        var l = Log.Fn<bool>($"create ghost a#{AppWorkCtx.Show()}, type:{sourceStaticName}");
        AppWorkCtx.DataController.ContentType.CreateGhost(sourceStaticName);
        return l.ReturnTrue();
    }


    public void SetTitle(int contentTypeId, int attributeId)
    {
        var l = Log.Fn($"set title type#{contentTypeId}, attrib:{attributeId}");
        AppWorkCtx.DataController.Attributes.SetTitleAttribute(attributeId, contentTypeId);
        l.Done();
    }

    public bool Delete(string staticName)
    {
        var l = Log.Fn<bool>($"delete a#{AppWorkCtx.Show()}, name:{staticName}");
        AppWorkCtx.DataController.ContentType.Delete(staticName);
        return l.ReturnTrue();
    }

}