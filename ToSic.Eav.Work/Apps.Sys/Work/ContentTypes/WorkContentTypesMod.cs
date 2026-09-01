using ToSic.Eav.Data.Processing;
using ToSic.Sys.Utils;

namespace ToSic.Eav.Apps.Sys.Work;

[ShowApiWhenReleased(ShowApiMode.Never)]
public class WorkContentTypesMod(
    LazySvc<ContentTypeChangeActionRunner> changeActions)
    : ServiceWithSetup<IAppWorkContext>("ApS.InpGet", connect: [changeActions])
{
    public void Create(string nameId, string scope)
    {
        var l = Log.Fn();
        var db = MyOptions.DbStorage;
        var ct = db.ContentTypes.PrepareDbContentType(nameId, nameId, scope, false, MyOptions.AppId);
        if (ct != null)
            db.DoAndSaveWithoutChangeDetection(() => db.SqlDb.Add(ct));
        l.Done();
    }

    public bool AddOrUpdate(string staticName, string scope, string name, int? usesConfigurationOfOtherSet,
        bool alwaysShareConfig)
    {
        var l = Log.Fn<bool>($"save {MyOptions.Show()}");
        if (name.IsEmptyOrWs())
            return l.ReturnFalse("name was empty, will cancel");

        MyOptions.NewDbStorage().ContentType.AddOrUpdate(
            staticName,
            scope,
            name,
            usesConfigurationOfOtherSet,
            alwaysShareConfig);
        // Schema changes on the type itself should immediately re-evaluate code-generation handlers.
        changeActions.Value.RunFor(
            MyOptions.AppId,
            staticName,
            source: ContentTypeChangeSources.ContentType);
        return l.ReturnTrue();
    }

    public bool CreateGhost(string sourceStaticName)
    {
        var l = Log.Fn<bool>($"create ghost a#{MyOptions.Show()}, type:{sourceStaticName}");
        MyOptions.NewDbStorage().ContentType.CreateGhost(sourceStaticName);
        return l.ReturnTrue();
    }


    public void SetTitle(int contentTypeId, int attributeId)
    {
        var l = Log.Fn($"set title type#{contentTypeId}, attrib:{attributeId}");
        MyOptions.NewDbStorage().Attributes.SetTitleAttribute(attributeId, contentTypeId);
        l.Done();
    }

    public bool Delete(string staticName)
    {
        var l = Log.Fn<bool>($"delete a#{MyOptions.Show()}, name:{staticName}");
        MyOptions.NewDbStorage().ContentType.Delete(staticName);
        return l.ReturnTrue();
    }

}
