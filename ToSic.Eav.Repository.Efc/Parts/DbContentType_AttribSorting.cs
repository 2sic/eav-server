﻿namespace ToSic.Eav.Repository.Efc.Parts;

partial class DbContentType
{
    /// <summary>
    /// Sometimes the import asks for sorting the fields again according to input
    /// this method will then take care of re-sorting them correctly
    /// Fields which were not in the import will simply land at the end
    /// </summary>
    /// <param name="contentTypeId"></param>
    /// <param name="contentType"></param>
    private void SortAttributes(int contentTypeId, IContentType contentType)
    {
        var attributeList = DbContext.SqlDb.ToSicEavAttributesInSets
            .Where(a => a.AttributeSetId == contentTypeId)
            .ToList();

        var ctAttribList = contentType.Attributes.ToList();
        attributeList = attributeList
            .OrderBy(a => ctAttribList
                .IndexOf(contentType.Attributes
                    .First(ia => ia.Name == a.Attribute.StaticName)))
            .ToList();
        PersistAttributeOrder(attributeList);
    }


    /// <summary>
    /// Update the order of the attributes in the set.
    /// </summary>
    /// <param name="contentTypeId"></param>
    /// <param name="newOrder">Array of attribute ids which defines the new sort order</param>
    public void SortAttributes(int contentTypeId, List<int> newOrder)
    {
        var attributeList = DbContext.SqlDb.ToSicEavAttributesInSets
            .Where(a => a.AttributeSetId == contentTypeId)
            .ToList();
        attributeList = attributeList
            .OrderBy(a => newOrder.IndexOf(a.AttributeId))
            .ToList();

        PersistAttributeOrder(attributeList);
    }

    private void PersistAttributeOrder(List<ToSicEavAttributesInSets> attributeList)
    {
        var index = 0;
        DbContext.DoAndSave(() => attributeList.ForEach(a => a.SortOrder = index++));
    }

}