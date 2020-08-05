using ToSic.Eav.ImportExport.Options;

namespace ToSic.Eav.WebApi.Formats
{
    public class ContentImportArgs
    {
        public int AppId;

        public string DefaultLanguage;

        public ImportResourceReferenceMode ImportResourcesReferences;

        public ImportDeleteUnmentionedItems ClearEntities;

        public string ContentType;

        public string ContentBase64;

        public string DebugInfo =>
            $"app:{AppId} + deflang:{DefaultLanguage}, + ct:{ContentType} + base:{ContentBase64}, impRes:{ImportResourcesReferences}, clear:{ClearEntities}";
    }
}
