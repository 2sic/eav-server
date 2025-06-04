using ToSic.Eav.ImportExport.Internal.Xml;
using ToSic.Eav.ImportExport.Sys.Options;
using ToSic.Eav.ImportExport.Sys.Xml;

namespace ToSic.Eav.ImportExport.Internal.XmlList;

partial class ImportListXml
{

    /// <summary>
    /// Get the languages found in the xml document.
    /// </summary>
    public IEnumerable<string> Info_LanguagesInDocument => DocumentElements
        .Select(element => element.Element(XmlConstants.EntityLanguage)?.Value)
        .Distinct();

    /// <summary>
    /// Get the attributes not imported (ignored) from the document to the repository.
    /// </summary>
    public IEnumerable<string> Info_AttributeNamesNotImported
    {
        get
        {
            var existingAttributes = Info_AttributeNamesInContentType;
            var creatdAttributes = Info_AttributeNamesInDocument;
            return existingAttributes.Except(creatdAttributes);
        }
    }


    /// <summary>
    /// The amount of enities created in the repository on data import.
    /// </summary>
    public int Info_AmountOfEntitiesCreated
    {
        get
        {
            var existingGuids = GetExistingEntityGuids();
            var createdGuids = GetCreatedEntityGuids();
            return createdGuids.Except(existingGuids).Count();
        }
    }

    /// <summary>
    /// The amount of enities updated in the repository on data import.
    /// </summary>
    public int Info_AmountOfEntitiesUpdated
    {
        get
        {
            var existingGuids = GetExistingEntityGuids();
            var createdGuids = GetCreatedEntityGuids();
            return createdGuids.Count(guid => existingGuids.Contains(guid));
        }
    }

    private List<Guid> GetEntityDeleteGuids()
    {
        var existingGuids = GetExistingEntityGuids();
        var createdGuids = GetCreatedEntityGuids();
        return existingGuids.Except(createdGuids).ToList();
    }

    /// <summary>
    /// The amount of enities deleted in the repository on data import.
    /// </summary>
    public int Info_AmountOfEntitiesDeleted => _deleteSetting == ImportDeleteUnmentionedItems.None ? 0 : GetEntityDeleteGuids().Count;

    #region Deserialize statistics methods
    private List<Guid> GetExistingEntityGuids()
    {
        var existingGuids = ExistingEntities
            .Select(entity => entity.EntityGuid).ToList();
        return existingGuids;
    }


    /// <summary>
    /// Get the attribute names in the content type.
    /// </summary>
    public IEnumerable<string> Info_AttributeNamesInContentType
        => ContentType.Attributes.Select(item => item.Name).ToList();

    #endregion Deserialize statistics methods


}