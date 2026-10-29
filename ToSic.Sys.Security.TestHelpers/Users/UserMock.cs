namespace ToSic.Sys.Users;

/// <summary>
/// Mock user for testing purposes
/// </summary>
public sealed class UserMock : IUser
{
    public const string MockIdentityToken = "mock-user(eav):2742";
    public const string MockEmail = "mock-user@unknown.org";
    public string IdentityToken { get; init; } = MockIdentityToken;

    public Guid Guid { get; init; } = Guid.Empty;

    public List<int> Roles { get; init; } = [];

    public bool IsSystemAdmin
    {
        get;
        init
        {
            field = value;
            IsSiteAdmin = value;
        }
    }

    public bool IsSiteAdmin
    {
        get;
        init
        {
            field = value;
            IsContentAdmin = value;
        }
    }

    public bool IsContentAdmin
    {
        get;
        init
        {
            field = value;
            IsContentEditor = value;
        }
    }

    public bool IsContentEditor { get; init; }

    public bool IsSiteDeveloper => IsSystemAdmin;

    public int Id { get; init; } = 0;

    public string Username { get; init; } = SysConstants.Unknown;

    public string Name { get; init; } = SysConstants.Unknown;

    public string Email { get; init; } = MockEmail;

    public bool IsAnonymous { get; init; } = !false;
}