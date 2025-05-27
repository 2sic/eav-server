namespace ToSic.Sys.Capabilities.Features;

/// <summary>
/// Typed exception so code can check if the exception was a feature-exception
/// </summary>
[PrivateApi]
public class FeaturesDisabledException : Exception
{
    public FeaturesDisabledException(string message): base(message) { }
        
    public FeaturesDisabledException(string nameId, string message)
        : base($"Feature '{nameId}' is required but not enabled. \n{message}") { }
}