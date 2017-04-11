using System;
using System.Xml.Linq;

namespace ToSic.Eav.ImportExport.Xml
{
    public class ImportItemGuidManager
    {
        private Guid _entityGuidLast = Guid.Empty;

        /// <summary>
        /// Get the entity GUID for a document element of the XML file (maybe the last GUID or the next 
        /// one... depends on some rules).
        /// </summary>
        public Guid GetGuid(XElement element, string languageFallback)
        {
            Guid entityGuid;

            var elementGuid = element.Element(XmlConstants.EntityGuid)?.Value;
            if (string.IsNullOrEmpty(elementGuid))
            {
                var elementLanguage = element.Element(XmlConstants.EntityLanguage)?.Value;
                if (elementLanguage == languageFallback || string.IsNullOrEmpty(elementLanguage)) 
                {   // If the element does not have a GUID and the element has data for the default 
                    // language, create a new GUID
                    entityGuid = Guid.NewGuid();
                }
                else
                {
                    entityGuid = _entityGuidLast;
                }
            }
            else
            {
                entityGuid = Guid.Parse(elementGuid);
            }

            _entityGuidLast = entityGuid;
            return entityGuid;
        }
    }
}