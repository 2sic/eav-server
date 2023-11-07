using ToSic.Lib.Logging;

namespace ToSic.Eav.Apps.Work
{
    public class WorkContentTypesMod : WorkUnitBase<IAppWorkCtxWithDb>
    {
        public WorkContentTypesMod() : base("ApS.InpGet")
        {
        }


        public void Create(string nameId, string scope) =>
            AppWorkCtx.DataController.DoAndSave(() =>
                AppWorkCtx.DataController.AttribSet.PrepareDbAttribSet(nameId, nameId, scope, false, AppWorkCtx.AppId));


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
}
