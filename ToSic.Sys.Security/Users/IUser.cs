namespace ToSic.Eav.Context;

[PrivateApi]
public interface IUser: ILogShouldNeverConnect
{

    /// <summary>
    /// User Id as int. Works in DNN and Oqtane
    /// </summary>
    int Id { get; }

    /// <summary>
    /// The identity token is usually something like "dnn:27" or similar.
    /// It's used in two places: for marking changes in the DB (so we know who did it) and as an identifier in the history (so the history can show who made changes).
    /// </summary>
    string IdentityToken { get; }

    /// <summary>
    /// An optional user GUID.
    /// On systems which don't give the user a unique guid, it will be `Guid.Empty`.
    /// </summary>
    Guid Guid { get; }

    /// <summary>
    /// UserName used for login.
    /// </summary>
    public string Username { get; }

    /// <summary>
    /// Nice name aka DisplayName.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// User E-Mail Address, aka PreferredEmail
    /// </summary>
    public string Email { get; }

    /// <summary>
    /// List of roles the user is in. This is used in permissions check.
    /// Still WIP - probably not the ideal interface, we'll probably change it to something else where the role must not be Int-based. 
    /// </summary>
    [WorkInProgressApi("This may still change, as it's probably better to implement something like IsInRole or something similar.")]
    public List<int> Roles { get; }

    /// <summary>
    /// True if the user has super-user rights.
    /// This kind of user can do everything, incl. create apps. 
    /// </summary>
    /// <remarks>
    /// Renamed in v14.09 from IsSuperUser to this to be consistent with other APIs.
    /// </remarks>
    public bool IsSystemAdmin { get; }


    /// <summary>
    /// True if the user is admin - allowing full content-management and user-management.
    /// </summary>
    /// <remarks>
    /// Renamed in v14.09 from IsAdmin to this to be consistent with other APIs.
    /// </remarks>
    public bool IsSiteAdmin { get; }

    /// <summary>
    /// True if the user is a content admin / editor.
    /// If the user only has this role, then he/she can only edit pages and content, but not users.
    /// In DNN 10, ContentAdmins can also publish pages.
    /// </summary>
    /// <remarks>New in v14.09</remarks>
    public bool IsContentAdmin { get; }

    /// <summary>
    /// Determines if the user is a content editor.
    /// In DNN 10, ContentEditors cannot publish pages (unless they are also ContentAdmins).
    /// </summary>
    /// <remarks>New in v18.02</remarks>
    public bool IsContentEditor { get; }

    /// <summary>
    /// True if a user is in the SxcDesigners group.
    /// Such a person can actually do a lot more, like access the advanced toolbars. 
    /// </summary>
    /// <remarks>
    /// Used to be called IsDesigner up until v15.03, but probably never used outside of core code
    /// </remarks>
    public bool IsSiteDeveloper { get; }

    /// <summary>
    /// True if the user is anonymous / not logged in. 
    /// </summary>
    public bool IsAnonymous { get; }

}