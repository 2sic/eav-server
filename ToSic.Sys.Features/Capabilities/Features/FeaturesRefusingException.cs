namespace ToSic.Sys.Capabilities.Features;

/// <summary>
/// Typed exception so code can check if the exception was a feature-exception
/// </summary>
[PrivateApi]
public class FeaturesRefusingException: Exception
{
    public FeaturesRefusingException(string message): base(message) { }
        
    public FeaturesRefusingException(string nameId, string message)
        : base($"Feature '{nameId}' is enabled, refusing this action. \n{message}") { }
}