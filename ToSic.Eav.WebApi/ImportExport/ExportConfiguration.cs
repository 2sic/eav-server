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
    public class ExportConfiguration : EntityBasedType
    {
        public const string ContentTypeId = "d7f2e4fa-5306-41bb-a3cd-d9529c838879";
        
        public ExportConfiguration(IEntity entity) : base(entity)
        {
        }

        /// <summary>
        /// Name of the configuration - just fyi
        /// </summary>
        public string Name => GetThis("");

        /// <summary>
        /// Determine if we should keep or remove the Export-Decorators when exporting
        /// </summary>
        public bool PreserveMarkers => GetThis(false);
        
        /// <summary>
        /// Expected file name.
        /// </summary>
        public string FileName => GetThis("bundle.json");

        // find all decorator metadata of type SystemExportDecorator
        // use the guid for finding them: 32698880-1c2e-41ab-bcfc-420091d3263f
        // filter by the Configuration field
        public List<ExportDecorator> ExportMarkers => _exportMarkers.Get(() => Entity.Parents(ExportDecorator.TypeNameId).Select(e => new ExportDecorator(e)).ToList());
        private readonly GetOnce<List<ExportDecorator>> _exportMarkers = new GetOnce<List<ExportDecorator>>();

        /// <summary>
        /// Content Types to Export
        /// </summary>
        public List<string> ContentTypes => ExportMarkers
            .Where(e => e.IsContentType && !string.IsNullOrEmpty(e.KeyString))
            .Select(et => et.KeyString).ToList();
        
        /// <summary>
        /// Entities to Export
        /// </summary>
        public List<Guid> Entities => ExportMarkers
            .Where(e => e.IsEntity && e.KeyGuid.HasValue)
            .Select(et => et.KeyGuid.Value).ToList();
    }
}
