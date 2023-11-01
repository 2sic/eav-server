using System.Collections.Generic;
using System.IO;
using ToSic.Eav.Apps.ImportExport;
using ToSic.Eav.ImportExport.Options;

namespace ToSic.Eav.Apps.Parts
{
    public partial class EntitiesManager
    {
        public ImportListXml Importer(
            string contentTypeName,
            Stream dataStream,
            IEnumerable<string> languages,
            string documentLanguageFallback,
            ImportDeleteUnmentionedItems deleteSetting,
            ImportResolveReferenceMode resolveReferenceMode)
        {
            var ct = Parent.AppState.GetContentType(contentTypeName);
            return _lazyImportListXml.Value.Init(Parent.AppState, ct,
                dataStream, languages, documentLanguageFallback,
                deleteSetting, resolveReferenceMode);
        }
    }
}
