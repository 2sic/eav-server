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
    /// <param name="fields"></param>
    /// <param name="index"></param>
    void Delete(Guid? parent, string fields, int index);

    // TODO: MOVE TO 2SXC
    // ReplacementListDto Replace(Guid guid, string part, int index);

}