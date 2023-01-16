using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Data;
using ToSic.Lib.Helper;

namespace ToSic.Eav.WebApi.ImportExport
{
    public class ExportConfiguration : EntityBasedType
    {
        public static string TypeNameId = Eav.Metadata.Decorators.SystemExportDecorator;
        public static string ContentType = "SystemExportDecorator";
        
        public ExportConfiguration(IEntity entity) : base(entity)
        {
        }

        public string Name => Get("Name", "");

        public bool PreserveMarkers => Get("PreserveMarkers", false);
        
        public string FileName => Get("FileName", "bundle.json");

        // find all decorator metadata of type SystemExportDecorator
        // use the guid for finding them: 32698880-1c2e-41ab-bcfc-420091d3263f
        // filter by the Configuration field
        public List<ExportMarker> ExportMarkers => _exportMarkers.Get(() => Entity.Parents(Eav.Metadata.Decorators.SystemExportDecorator).Select(e => new ExportMarker(e)).ToList());
        private readonly GetOnce<List<ExportMarker>> _exportMarkers = new GetOnce<List<ExportMarker>>();

        public List<string> ContentTypes => ExportMarkers
            .Where(e => e.IsContentType && !string.IsNullOrEmpty(e.KeyString))
            .Select(et => et.KeyString).ToList();
        
        public List<Guid> Entities => ExportMarkers
            .Where(e => e.IsEntity && e.KeyGuid.HasValue)
            .Select(et => et.KeyGuid.Value).ToList();
    }
}
