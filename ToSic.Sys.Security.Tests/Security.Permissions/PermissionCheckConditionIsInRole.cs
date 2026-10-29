using Microsoft.Extensions.DependencyInjection.Extensions;
using ToSic.Sys.Capabilities.Features;
using ToSic.Sys.DI;
using ToSic.Sys.Users;

namespace ToSic.Sys.Security.Permissions;

public class PermissionCheckConditionIsInRole(Generator<PermissionCheck, PermissionCheck.Options> permCheck, ISysFeaturesService featuresService)
{
    private const int RoleIgnored = 97;
    private const int RoleUsed = 42;
    private const int RoleOtherUser = 99;

    /// <summary>
    /// Setup DI.
    /// </summary>
    public class Startup() : QuickStartup(s =>
    {
        // The mock user needs a real guid, otherwise it won't be considered for deeper permission checks.
        s.TryAddTransient<IUser>(_ => new UserMock { Guid = Guid.NewGuid(), Roles = [RoleIgnored, RoleUsed] });
        s.SetupSysSecurity();
    });

    private static readonly Dictionary<string, List<IPermission>> Scenarios = new()
    {
        { "none", [] },
        { "owner-only", [new MockPermission("", UserMock.MockIdentityToken, "r") { Identity = RoleUsed.ToString() }] },
        { "owner-mixed", [new MockPermission("", UserMock.MockIdentityToken, "r") { Identity = $"{RoleOtherUser},{RoleUsed}" }] },
        { "other-user", [new MockPermission("", UserMock.MockIdentityToken, "r") { Identity = RoleOtherUser.ToString() }] },
        { "invalid-number", [new MockPermission("", UserMock.MockIdentityToken, "r") { Identity = "invalid-number" }] },
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
    [InlineData(false, true, "none", UserMock.MockIdentityToken)]
    [InlineData(true, true, "owner-only", UserMock.MockIdentityToken)]
    [InlineData(false, false, "owner-only", UserMock.MockIdentityToken)]
    [InlineData(true, true, "owner-mixed", UserMock.MockIdentityToken)]
    [InlineData(false, false, "owner-mixed", UserMock.MockIdentityToken)]
    [InlineData(false, true, "other-user", UserMock.MockIdentityToken)]
    [InlineData(false, false, "other-user", UserMock.MockIdentityToken)]
    [InlineData(false, true, "invalid-number", UserMock.MockIdentityToken)]
    [InlineData(false, false, "invalid-number", UserMock.MockIdentityToken)]
    public void IsAllowed(bool expected, bool activateFeature, string scenarioName, string userIdentity)
    {
        // Arrange
        if (activateFeature)
            ActivatePermissionCheckGroups();
        else
            featuresService.UpdateFeatureList(new(), []);

        // Assert
        Equal(expected, CheckRead(scenarioName, userIdentity).Allowed);
    }

    private void ActivatePermissionCheckGroups() =>
        featuresService.UpdateFeatureList(new()
            {
                Features =
                [
                    new() { Id = BuiltInFeatures.PermissionCheckGroups.Guid, Enabled = true },
                ],
            },
            []
        );


    [Fact]
    public void IdentityReasonIsIdentity()
    {
        ActivatePermissionCheckGroups();
        Equal(Conditions.Group, CheckRead("owner-only", UserMock.MockIdentityToken).Condition);
    }
}