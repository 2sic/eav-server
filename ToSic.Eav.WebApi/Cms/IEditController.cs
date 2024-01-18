using ToSic.Eav.WebApi.Formats;

namespace ToSic.Eav.WebApi.Cms;

/// <summary>
/// This is the Contract / Interface for the edit controller.
/// All WebAPI implementations of Edit should be based on this, so we can tell at compile time if they are compatible
/// </summary>
public interface IEditController
{
    /// <summary>
    /// Load data for the edit UI. 
    /// Always a POST method.
    /// </summary>
    /// <param name="items">Identifier objects telling the backend what to provide - usually in the POST.</param>
    /// <param name="appId">AppId of the app we're working in</param>
    /// <returns>Complex object containing the edit-data</returns>
    EditDto Load(List<ItemIdentifier> items, int appId);

    /// <summary>
    /// Save the data from the edit UI.
    /// Always a POST method. 
    /// </summary>
    /// <param name="package">Bundle of data to be saved, usually in the POST</param>
    /// <param name="appId">Current App ID</param>
    /// <param name="partOfPage">Tell the save operation if this may be tied to a page-publishing workflow</param>
    /// <returns></returns>
    Dictionary<Guid, int> Save(EditDto package, int appId, bool partOfPage);

    ///// <summary>
    ///// Used to be GET or POST Ui/GetAvailableEntities.
    ///// By default it uses GET, but usually the items-array is passed in through a POST.
    ///// </summary>
    ///// <param name="appId"></param>
    ///// <param name="contentTypeName"></param>
    ///// <param name="items"></param>
    ///// <returns></returns>
    //// security check happens internally
    // interface method is commented because aspnetcore implementation have different signature
    // IEnumerable<EntityForPickerDto> EntityPicker(int appId, string contentTypeName = null);


    /// <summary>
    /// This GET-call will resolve links to files and to pages.
    /// For page-resolving, it only needs Hyperlink and AppId.
    /// For file resolves it needs the item context so it can verify that an item is in the ADAM folder of this object.
    /// </summary>
    /// <param name="link">The link to resolve - required. Can be a real link or a file:xx page:xx reference</param>
    /// <param name="appId">App id to which this link (or or file/page) belongs to</param>
    /// <param name="contentType">Content Type (optional). Relevant for checking ADAM links inside an item.</param>
    /// <param name="guid">Item GUID (optional). Relevant for checking ADAM links inside an item.</param>
    /// <param name="field">Item field (optional). Relevant for checking ADAM links inside an item.</param>
    /// <returns></returns>
    LinkInfoDto LinkInfo(string link, int appId, string contentType = default, Guid guid = default, string field = default);

    /// <summary>
    /// Used to be GET Module/Publish
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    bool Publish(int id);
}