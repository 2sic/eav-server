using Microsoft.Extensions.DependencyInjection.Extensions;
using ToSic.Sys.Capabilities.Features;
using ToSic.Sys.DI;
using ToSic.Sys.Users;

namespace ToSic.Sys.Security.Permissions;

public class PermissionCheckConditionIsUser(Generator<PermissionCheck, PermissionCheck.Options> permCheck, ISysFeaturesService featuresService)
{
    /// <summary>
    /// Guid to use to identify the "current" user and the rule which is applied
    /// </summary>
    private static readonly Guid UserGuid = Guid.NewGuid();

    /// <summary>
    /// Setup DI.
    /// </summary>
    public class Startup() : QuickStartup(s =>
    {
        // The mock user needs a real guid, otherwise it won't be considered for deeper permission checks.
        s.TryAddTransient<IUser>(_ => new UserMock { Guid = UserGuid });
        s.SetupSysSecurity();
    });

    private static readonly Dictionary<string, List<IPermission>> Scenarios = new()
    {
        { "none", [] },
        { "owner-only", [new MockPermission("", UserMock.MockIdentityToken, "r") { Identity = UserGuid.ToString() }] },
        { "other-user", [new MockPermission("", UserMock.MockIdentityToken, "r") { Identity = Guid.NewGuid().ToString() }] },
        { "invalid-guid", [new MockPermission("", UserMock.MockIdentityToken, "r") { Identity = "invalid-guid" }] },
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
    [InlineData(false, true, "other-user", UserMock.MockIdentityToken)]
    [InlineData(false, false, "other-user", UserMock.MockIdentityToken)]
    [InlineData(false, true, "invalid-guid", UserMock.MockIdentityToken)]
    [InlineData(false, false, "invalid-guid", UserMock.MockIdentityToken)]
    public void IsAllowed(bool expected, bool activateFeature, string scenarioName, string userIdentity)
    {
        // Arrange
        if (activateFeature)
            ActivatePermissionCheckUsers();
        else
            featuresService.UpdateFeatureList(new(), []);

        // Assert
        Equal(expected, CheckRead(scenarioName, userIdentity).Allowed);
    }

    private void ActivatePermissionCheckUsers() =>
        featuresService.UpdateFeatureList(new()
            {
                Features =
                [
                    new() { Id = BuiltInFeatures.PermissionCheckUsers.Guid, Enabled = true },
                ],
            },
            []
        );


    [Fact]
    public void IdentityReasonIsIdentity()
    {
        ActivatePermissionCheckUsers();
        Equal(Conditions.Identity, CheckRead("owner-only", UserMock.MockIdentityToken).Condition);
    }
}