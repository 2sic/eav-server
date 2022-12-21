using ToSic.Lib.Documentation;

namespace ToSic.Eav.Data
{
    public partial class ContentType
    {

        #region Helpers just for creating ContentTypes which will be imported
        [PrivateApi]
        // #RemoveContentTypeDescription #2974 - #remove ca. Feb 2023 if all works
        public void SetImportParameters(string scope, string nameId, /*string description,*/ bool alwaysShareDef)
        {
            Scope = Scopes.RenameOldScope(scope);
            NameId = nameId;
            // #RemoveContentTypeDescription #2974 - #remove ca. Feb 2023 if all works
            //Description = description;
            AlwaysShareConfiguration = alwaysShareDef;
        }

        // special values just needed for import / save 
        // todo: try to place in a sub-object to un-clutter this ContentType object
        [PrivateApi]
        public bool OnSaveSortAttributes { get; set; } = false;

        [PrivateApi]
        public string OnSaveUseParentStaticName { get; set; }


        #endregion
    }
}
