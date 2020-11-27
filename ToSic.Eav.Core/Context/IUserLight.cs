using ToSic.Eav.Documentation;

namespace ToSic.Eav.Context
{
    /// <summary>
    /// Public properties of the IUser for use in your own code
    /// </summary>
    [PublicApi]
    public interface IUserLight
    {
        /// <summary>
        /// User ID
        /// </summary>
        int Id { get; }

    }
}
