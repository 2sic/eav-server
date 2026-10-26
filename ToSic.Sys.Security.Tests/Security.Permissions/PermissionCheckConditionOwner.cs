using Microsoft.Extensions.DependencyInjection.Extensions;
using ToSic.Sys.DI;
using ToSic.Sys.Users;

namespace ToSic.Sys.Security.Permissions;

public class PermissionCheckConditionOwner(Generator<PermissionCheck, PermissionCheck.Options> permCheck)
{
    /// <summary>
    /// Setup DI.
    /// </summary>
    public class Startup() : QuickStartup(s =>
    {
        // The mock user needs a real guid, otherwise it won't be considered for deeper permission checks.
        s.TryAddTransient<IUser>(_ => new UserMock { Guid = Guid.NewGuid() });
        s.SetupSysSecurity();
    });

    private static readonly Dictionary<string, List<IPermission>> Scenarios = new()
    {
        {
            "none",
            []
        },
        {
            "no-owner1",
            [new MockPermission("", UserMock.MockIdentityToken, "r")]
        },
        {
            "no-owner3",
            [
                new MockPermission("", UserMock.MockIdentityToken, "r"),
                new MockPermission("y", UserMock.MockIdentityToken, "r"),
                new MockPermission("z", UserMock.MockIdentityToken, "d")
            ]
        },
        {
            "owner-only",
            [new MockPermission(nameof(IPermission.Owner), UserMock.MockIdentityToken, "r")]
        },
        {
            "owner-somewhere",
            [
                new MockPermission("", UserMock.MockIdentityToken, "d"),
                new MockPermission(nameof(IPermission.Owner), UserMock.MockIdentityToken, "r"),
                new MockPermission("z", UserMock.MockIdentityToken, "r"),
            ]
        },
        {
            "owner-but-not-read",
            [new MockPermission(nameof(IPermission.Owner), UserMock.MockIdentityToken, "d")]
        },
    };

    private PermissionCheckInfo CheckRead(string scenarioName, string targetIdentity) =>
        permCheck
            .New(new(
                Permissions: Scenarios[scenarioName],
                TargetOwner: targetIdentity,
                User: null,
                Env: null)
            )
            .UserMay([Grants.Read]);


    [Theory]
    [InlineData(false, "none", UserMock.MockIdentityToken)]
    [InlineData(false, "no-owner1", UserMock.MockIdentityToken)]
    [InlineData(false, "no-owner3", UserMock.MockIdentityToken)]
    [InlineData(true, "owner-only", UserMock.MockIdentityToken)]
    [InlineData(true, "owner-somewhere", UserMock.MockIdentityToken)]
    [InlineData(false, "owner-but-not-read", UserMock.MockIdentityToken)]
    [InlineData(false, "none", "non-owner")]
    [InlineData(false, "owner-only", "non-owner")]
    public void IsAllowed(bool expected, string scenarioName, string userIdentity)
        => Equal(expected, CheckRead(scenarioName, userIdentity).Allowed);


    [Fact]
    public void OwnerReasonIsOwner()
        => Equal(Conditions.Owner, CheckRead("owner-only", UserMock.MockIdentityToken).Condition);

}