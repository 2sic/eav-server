using ToSic.Eav.Documentation;
using ToSic.Eav.Logging;

namespace ToSic.Eav.LookUp
{
    /// <summary>
    /// An object implementing this interface can provide an engine for the current context. <br/>
    /// It's important so that code can easily ask for the current engine, but that the
    /// real implementation is dependency-injected later on, as each environment (DNN, Nop, etc.)
    /// can provide different initial engines. <br/>
    /// Read more about this in @Specs.LookUp.Intro
    /// </summary>
    [PublicApi_Stable_ForUseInYourCode]
    public interface IGetEngine
    {
        /// <summary>
        /// Get the engine for the current execution instance.
        /// </summary>
        /// <param name="instanceId">The instance ID</param>
        /// <param name="parentLog">parent logger</param>
        /// <returns>a <see cref="ILookUpEngine"/> for the current context</returns>
        ILookUpEngine GetEngine(int instanceId, ILog parentLog);

    }
}
