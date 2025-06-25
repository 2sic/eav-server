namespace ToSic.Eav.ImportExport.Sys.XmlList;

partial class ImportListXml
{
    [field: AllowNull, MaybeNull]
    public XmlImportPreparations Preparations
    {
        get => field ?? throw new InvalidOperationException("Statistics not initialized. Call GetStats() first.");
        private set;
    } = null!;

    ///// <summary>
    ///// Get the languages found in the xml document.
    ///// </summary>
    //public IEnumerable<string?> GetInfo_LanguagesInDocument(IEnumerable<XElement> xmlEntities)
    //    => xmlEntities
    //        .Select(element => element.Element(XmlConstants.EntityLanguage)?.Value)
    //        .Distinct();

    ///// <summary>
    ///// Get the attributes not imported (ignored) from the document to the repository.
    ///// </summary>
    //public IEnumerable<string> GetInfo_AttributeNamesNotImported(IEnumerable<XElement> xmlEntities)
    //{
    //    var existingAttributes = Info_AttributeNamesInContentType;
    //    var createdAttributes = GetInfo_AttributeNamesInDocument(xmlEntities);
    //    return existingAttributes.Except(createdAttributes);
    //}


    ///// <summary>
    ///// The amount of entities created in the repository on data import.
    ///// </summary>
    //public int Info_AmountOfEntitiesCreated
    //{
    //    get
    //    {
    //        var existingGuids = GetExistingEntityGuids();
    //        var createdGuids = GetCreatedEntityGuids();
    //        return createdGuids.Except(existingGuids).Count();
    //    }
    //}

    ///// <summary>
    ///// The amount of enities updated in the repository on data import.
    ///// </summary>
    //public int Info_AmountOfEntitiesUpdated
    //{
    //    get
    //    {
    //        var existingGuids = GetExistingEntityGuids();
    //        var createdGuids = GetCreatedEntityGuids();
    //        return createdGuids.Count(guid => existingGuids.Contains(guid));
    //    }
    //}

    private List<Guid> GetEntityDeleteGuids(List<Guid> existingGuids, List<Guid> createdGuids)
    {
        //var existingGuids = GetExistingEntityGuids();
        //var createdGuids = GetCreatedEntityGuids();
        return existingGuids
            .Except(createdGuids)
            .ToList();
    }

    ///// <summary>
    ///// The amount of enities deleted in the repository on data import.
    ///// </summary>
    //public int Info_AmountOfEntitiesDeleted
    //    => _deleteSetting == ImportDeleteUnmentionedItems.None ? 0 : GetEntityDeleteGuids().Count;

    #region Deserialize statistics methods
    private List<Guid> GetExistingEntityGuids(List<IEntity> existingEntities)
    {
        var existingGuids = existingEntities
            .Select(entity => entity.EntityGuid)
            .ToList();
        return existingGuids;
    }


    ///// <summary>
    ///// Get the attribute names in the content type.
    ///// </summary>
    //public IEnumerable<string> AttributeNamesInContentType
    //    => ContentType.Attributes.Select(item => item.Name).ToList();

    #endregion Deserialize statistics methods


}