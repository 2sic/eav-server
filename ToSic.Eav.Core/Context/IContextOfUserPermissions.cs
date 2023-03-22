namespace ToSic.Eav.Context
{
    /// <summary>
    /// WIP 15.04
    /// </summary>
    public interface IContextOfUserPermissions
    {
        /// <summary>
        /// Determines if the user is regarded as an editor within this context.
        /// Will vary depending on how much we know about the user. In the Site-context it only depends
        /// on user permissions.
        /// </summary>
        bool UserMayEdit { get; }
    }
}
