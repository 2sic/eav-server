using ToSic.Eav.Documentation;

namespace ToSic.Eav.Run
{
    /// <summary>
    /// Carries information about what compatibility level to use. Important for components that have an older and newer API.
    /// </summary>
    [InternalApi_DoNotUse_MayChangeWithoutNotice("this is just fyi")]
    public interface ICompatibilityLevel
    {
        /// <summary>
        /// The compatibility level to use. 
        /// </summary>
        int CompatibilityLevel { get; }
    }
}
