using System.Collections.Generic;
using System.IO;
using ToSic.Eav.Apps.ImportExport;
using ToSic.Eav.ImportExport.Options;

namespace ToSic.Eav.Apps.Parts
{
    public partial class EntitiesManager
    {
        public ExportListXml Exporter(string contentType)
            => new ExportListXml(AppManager.AppState, AppManager.Read.ContentTypes.Get(contentType), Log);

        public ImportListXml Importer(
            string contentTypeName,
            Stream dataStream,
            IEnumerable<string> languages,
            string documentLanguageFallback,
            ImportDeleteUnmentionedItems deleteSetting,
            ImportResolveReferenceMode resolveReferenceMode)
        {
            var ct = AppManager.Read.ContentTypes.Get(contentTypeName);
            return new ImportListXml(AppManager, ct,
                dataStream, languages, documentLanguageFallback,
                deleteSetting, resolveReferenceMode, Log);
        }
    }
}
