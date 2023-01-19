using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Data;
using ToSic.Lib.Helper;

namespace ToSic.Eav.WebApi.ImportExport
{
    /// <summary>
    /// Metadata decorator for entities / content-types to mark them for export in a bundle
    /// </summary>
    public class ExportDecorator : EntityBasedType
    {
        public static string TypeNameId = "32698880-1c2e-41ab-bcfc-420091d3263f";
        public static string ContentType = "SystemExportDecorator";
        
        public ExportDecorator(IEntity entity) : base(entity)
        {
        }

        public string Name => GetThis("");

        public bool PreserveMarkers => GetThis(false);
        
        public string FileName => GetThis("bundle.json");

        // find all decorator metadata of type SystemExportDecorator
        // use the guid for finding them: 32698880-1c2e-41ab-bcfc-420091d3263f
        // filter by the Configuration field
        public List<ExportMarker> ExportMarkers => _exportMarkers.Get(() => Entity.Parents(TypeNameId).Select(e => new ExportMarker(e)).ToList());
        private readonly GetOnce<List<ExportMarker>> _exportMarkers = new GetOnce<List<ExportMarker>>();

        public List<string> ContentTypes => ExportMarkers
            .Where(e => e.IsContentType && !string.IsNullOrEmpty(e.KeyString))
            .Select(et => et.KeyString).ToList();
        
        public List<Guid> Entities => ExportMarkers
            .Where(e => e.IsEntity && e.KeyGuid.HasValue)
            .Select(et => et.KeyGuid.Value).ToList();
    }
}
