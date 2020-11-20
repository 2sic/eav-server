using ToSic.Eav.ImportExport.Options;

namespace ToSic.Eav.WebApi.Dto
{
    public class ContentImportArgsDto
    {
        public int AppId;

        public string DefaultLanguage;

        public ImportResolveReferenceMode ImportResourcesReferences;

        public ImportDeleteUnmentionedItems ClearEntities;

        public string ContentType;

        public string ContentBase64;

        public string DebugInfo =>
            $"app:{AppId} + deflang:{DefaultLanguage}, + ct:{ContentType} + base64:(not shown), impRes:{ImportResourcesReferences}, clear:{ClearEntities}";
    }
}
