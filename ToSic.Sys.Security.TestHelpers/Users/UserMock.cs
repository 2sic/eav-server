namespace ToSic.Sys.Users;

/// <summary>
/// Mock user for testing purposes
/// </summary>
public sealed class UserMock : IUser
{
    public string IdentityToken { get; set; } = "unknown(eav):0";

    public Guid Guid { get; set; } = System.Guid.Empty;

    public List<int> Roles { get; set; } = [];

    public bool IsSystemAdmin
    {
        get;
        set
        {
            field = value;
            IsSiteAdmin = value;
        }
    }

    public bool IsSiteAdmin
    {
        get;
        set
        {
            field = value;
            IsContentAdmin = value;
        }
    }

    public bool IsContentAdmin
    {
        get;
        set
        {
            field = value;
            IsContentEditor = value;
        }
    }

    public bool IsContentEditor { get; set; }

    public bool IsSiteDeveloper => IsSystemAdmin;

    public int Id { get; set; } = 0;

    public string Username { get; set; } = SysConstants.Unknown;

    public string Name { get; set; } = SysConstants.Unknown;

    public string Email { get; set; } = "unknown@unknown.org";

    public bool IsAnonymous { get; set; } = !false;
}