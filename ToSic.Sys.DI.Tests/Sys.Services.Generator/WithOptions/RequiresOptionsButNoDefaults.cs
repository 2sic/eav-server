using ToSic.Sys.DI;

namespace ToSic.Sys.Services.Generator.WithOptions;

/// <summary>
/// Test various use cases where the Service requires options,
/// but explicitly needs them to be handed in.
/// The service will fail to work without these options - which is a very common scenario.
/// </summary>
public class RequiresOptionsButNoDefaults(
    Generator<MockServiceReqOptionsNoDefaults> generateReqNoDefaults,
    Generator<MockServiceReqOptionsNoDefaults, MockServiceOptions> genReqNoDefaultsWithOptions,
    Generator<MockServiceReqOptionsAccessingInConstructor> generateReqNoDefaultsAccessingInConstructor,
    Generator<MockServiceReqOptionsAccessingInConstructor, MockServiceOptions> genReqNoDefaultsAccessingInConstructorWithOptions
)
{
    [Fact]
    public void ReqOptions_MissingOptions_Throws()
        => Throws<ServiceOptionsRequiredException>(() => generateReqNoDefaults.New().AccessOptions);

    [Fact]
    public void ReqOptions_WithOptions_Works()
        => Equal(MockServiceOptions.NameUndefined, genReqNoDefaultsWithOptions.New(new()).AccessOptions);
    
    [Fact]
    public void ReqOptions_AccessingInConstructor_Throws() =>
        Throws<ServiceOptionsRequiredException>(generateReqNoDefaultsAccessingInConstructor.New);

    [Fact]
    public void ReqOptions_AccessingInConstructor_WithOptions_Throws() =>
        Throws<ServiceOptionsRequiredException>(() => genReqNoDefaultsAccessingInConstructorWithOptions.New(new()));
}