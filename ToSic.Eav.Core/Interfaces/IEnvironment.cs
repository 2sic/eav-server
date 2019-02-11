namespace ToSic.Eav.Interfaces
{
    /// <summary>
    /// Any object implementing this interface can provide the EAV with information about the environment it's running in.
    /// </summary>
    public interface IEnvironment
    {
        /// <summary>
        /// The primary language in the current environment. 
        /// This is important for value-fallback, as not-translated data
        /// will try to revert to the primary language of the environment
        /// </summary>
        string DefaultLanguage { get; }

        /// <summary>
        /// The current user in the environment. 
        /// </summary>
        IUser User { get; }
    }
}
