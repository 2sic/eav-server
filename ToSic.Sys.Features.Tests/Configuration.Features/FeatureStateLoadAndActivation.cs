using ToSic.Sys.Capabilities.Features;

namespace ToSic.Sys.Features.Configuration.Features;

/// <summary>
/// Test feature activation state when loading changes the state.
/// </summary>
/// <remarks>
/// Since this service is shared and relies on static data,
/// each test call must reset the load, otherwise tests can affect other tests. 
/// </remarks>
/// <param name="featuresService"></param>
public class FeatureStateLoadAndActivation(ISysFeaturesService featuresService)
{
    public class Startup() : QuickStartup(sc => sc.AddSysCapabilitiesAndSysCore());

    /// <summary>
    /// This is a feature guid, which is known to be disabled by default.
    /// </summary>
    private static readonly Guid FeatGuidDisabledByDefault = BuiltInFeatures.EditUiAllowDebugModeForEditors.Guid;

    /// <summary>
    /// Verify that by default, the feature is not enabled.
    /// </summary>
    [Fact]
    public void DefaultIsNotEnabled()
    {
        // Arrange
        // Load/reset the features list to ensure it's in a blank state
        // Otherwise the state could be cached from previous tests and cause false positives
        featuresService.UpdateFeatureList(new(), []);

        // Act
        var enabled = featuresService.IsEnabled(FeatGuidDisabledByDefault);

        // Assert
        False(enabled);
    }

    /// <summary>
    /// Verify that if the loading process enables the feature, it's enabled.
    /// </summary>
    [Fact]
    public void WithLoadItsEnabled()
    {
        // Arrange
        featuresService.UpdateFeatureList(new()
            {
                Features =
                [
                    new() { Id = FeatGuidDisabledByDefault, Enabled = true },
                ],
            },
            []
        );

        // Act
        var enabled = featuresService.IsEnabled(FeatGuidDisabledByDefault);

        // Assert
        True(enabled);
    }
}
