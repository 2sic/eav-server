using System.Xml.Linq;
using ToSic.Eav.ImportExport.Sys.Options;
using ToSic.Eav.ImportExport.Sys.Xml;

namespace ToSic.Eav.ImportExport.Sys.XmlList;
public class XmlImportPreparations(
    IList<XElement> xmlEntities,
    IContentType contentType,
    List<IEntity> existingEntities,
    ImportDeleteUnmentionedItems deleteSetting,
    List<Guid> existingEntityGuids,
    List<Guid> createdEntityGuids,
    List<Guid> entityDeleteGuids)
{
    public int Count => xmlEntities.Count;

    public List<Guid> EntityDeleteGuids { get; } = entityDeleteGuids;

    public List<IEntity> ExistingEntities { get; } = existingEntities;

    /// <summary>
    /// Get the languages found in the xml document.
    /// </summary>
    public IEnumerable<string?> LanguagesInDocument
        => xmlEntities?
            .Select(element => element.Element(XmlConstants.EntityLanguage)?.Value)
            .Distinct()
        ?? [];

    /// <summary>
    /// The amount of entities deleted in the repository on data import.
    /// </summary>
    public int DeleteCount
        => deleteSetting == ImportDeleteUnmentionedItems.None ? 0 : entityDeleteGuids.Count;

    /// <summary>
    /// The amount of entities created in the repository on data import.
    /// </summary>
    public int EntitiesToCreate
    {
        get
        {
            var existingGuids = existingEntityGuids;
            var createdGuids = createdEntityGuids;
            return createdGuids.Except(existingGuids).Count();
        }
    }

    /// <summary>
    /// Get the attributes not imported (ignored) from the document to the repository.
    /// </summary>
    public IEnumerable<string> AttributeNamesNotImported
    {
        get
        {
            var existingAttributes = AttributeNamesInContentType;
            var createdAttributes = AttributeNamesInDocument;
            return existingAttributes.Except(createdAttributes);
        }
    }

    /// <summary>
    /// The amount of enities updated in the repository on data import.
    /// </summary>
    public int EntitiesToUpdate
    {
        get
        {
            var existingGuids = existingEntityGuids;
            var createdGuids = createdEntityGuids;
            return createdGuids.Count(guid => existingGuids.Contains(guid));
        }
    }

    /// <summary>
    /// Get the attribute names in the content type.
    /// </summary>
    public IEnumerable<string> AttributeNamesInContentType
        => contentType.Attributes.Select(item => item.Name).ToList();


    /// <summary>
    /// Get the attribute names in the xml document.
    /// </summary>
    public IEnumerable<string> AttributeNamesInDocument =>
        xmlEntities
            .SelectMany(element => element.Elements())
            .GroupBy(attribute => attribute.Name.LocalName)
            .Select(group => group.Key)
            .Where(name => name != XmlConstants.EntityGuid && name != XmlConstants.EntityLanguage)
            .ToList();

}
