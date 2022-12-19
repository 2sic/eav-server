using System.Collections.Generic;
using System.IO;
using ToSic.Eav.Apps.ImportExport;
using ToSic.Eav.ImportExport.Options;

namespace ToSic.Eav.Apps.Parts
{
    public partial class EntitiesManager
    {
        public ExportListXml Exporter(string contentType)
            => _exportListXmGenerator.New().Init(Parent.AppState, Parent.Read.ContentTypes.Get(contentType));

        public ImportListXml Importer(
            string contentTypeName,
            Stream dataStream,
            IEnumerable<string> languages,
            string documentLanguageFallback,
            ImportDeleteUnmentionedItems deleteSetting,
            ImportResolveReferenceMode resolveReferenceMode)
        {
            var ct = Parent.Read.ContentTypes.Get(contentTypeName);
            return _lazyImportListXml.Value.Init(Parent, ct,
                dataStream, languages, documentLanguageFallback,
                deleteSetting, resolveReferenceMode);
        }
    }
}
