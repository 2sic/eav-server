namespace ToSic.Eav.WebApi.Sys.Cms;

public interface IListController
{
    /// <summary>
    /// used to be GET Module/ChangeOrder
    /// </summary>
    /// <param name="parent"></param>
    /// <param name="fields"></param>
    /// <param name="index"></param>
    /// <param name="toIndex"></param>
    void Move(Guid? parent, string fields, int index, int toIndex);

    /// <summary>
    /// Used to be Get Module/RemoveFromList
    /// </summary>
    /// <param name="parent"></param>
    /// <param name="part"></param>
    /// <param name="index"></param>
    void Delete(Guid? parent, string part, int index);

    void Replace(Guid parent, string part, int index, int entityId, bool add = false);
    ReplacementListDto ReplaceOptions(Guid parent, string part, int index, string? contentType = null);

    List<EntityInListDto> Items(Guid parent, string part);
    bool Items(Guid parent, List<EntityInListDto> list, string part);

    List<EntityInListDto> ContentBlockHeader(Guid parent);
}