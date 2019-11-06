using ToSic.Eav.Documentation;

namespace ToSic.Eav.Data
{
    /// <remarks>
    /// Still a private API, because the naming / typing could change
    /// </remarks>
    /// <typeparam name="T"></typeparam>
    [PrivateApi]
    public interface IPublish<T>
    {
        /// <summary>
        /// Gets the RepositoryId - which is the ID in the database.
        /// This is usually the same as the EntityId, but sometimes differs,
        /// because when both a draft and published Entity exist, the have the same EntityId,
        /// but are stored with an own RepositoryId.
        /// </summary>
        int RepositoryId { get; }

        /// <summary>
        /// Indicates whether this Entity is Published (true) or a Draft (false)
        /// </summary>
        bool IsPublished { get; }

        /// <summary>
        /// Get Draft Entity of this Entity
        /// </summary>
        T GetDraft();

        /// <summary>
        /// Get Published Entity of this Entity
        /// </summary>
        T GetPublished();
    }
}
