using System;
using System.Xml.Linq;

namespace ToSic.Eav.ImportExport.Internal.Xml;

/// <summary>
/// Helper to find the current guid OR generate a new one
/// If it generated a new one, it will try to give the same GUID again for the next
/// imported line which appears to be the same item.
/// Only when another line of the primary language is parsed, will it regenerate a fresh guid.
/// </summary>
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
internal class ImportItemGuidManager
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
            // If the element does not have a GUID and the element has data for the default
            entityGuid = elementLanguage == languageFallback || string.IsNullOrEmpty(elementLanguage)
                ? Guid.NewGuid()
                : _entityGuidLast;
        }
        else
            entityGuid = Guid.Parse(elementGuid);

        _entityGuidLast = entityGuid;
        return entityGuid;
    }
}