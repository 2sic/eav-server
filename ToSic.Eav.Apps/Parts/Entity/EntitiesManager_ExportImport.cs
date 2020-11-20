using System.Collections.Generic;
using System.IO;
using ToSic.Eav.Apps.ImportExport;
using ToSic.Eav.ImportExport.Options;
using ToSic.Eav.Plumbing;

namespace ToSic.Eav.Apps.Parts
{
    public partial class EntitiesManager
    {
        public ExportListXml Exporter(string contentType)
            => Parent.ServiceProvider.Build<ExportListXml>().Init(Parent.AppState, Parent.Read.ContentTypes.Get(contentType), Log);

        public ImportListXml Importer(
            string contentTypeName,
            Stream dataStream,
            IEnumerable<string> languages,
            string documentLanguageFallback,
            ImportDeleteUnmentionedItems deleteSetting,
            ImportResolveReferenceMode resolveReferenceMode)
        {
            var ct = Parent.Read.ContentTypes.Get(contentTypeName);
            return /*new ImportListXml*/ _lazyImportListXml.Value.Init(Parent, ct,
                dataStream, languages, documentLanguageFallback,
                deleteSetting, resolveReferenceMode, Log);
        }
    }
}
