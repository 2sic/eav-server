namespace ToSic.Eav.Interfaces
{
    public interface IPublish<T>
    {
        /// <summary>
        /// Gets the RepositoryId
        /// </summary>
        int RepositoryId { get; }

        #region Published / Draft properties
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
        #endregion

    }
}
