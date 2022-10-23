using ToSic.Lib.Documentation;

namespace ToSic.Eav.Data
{
    public partial class Entity
    {
        /// <inheritdoc />
        [PrivateApi]
        public int RepositoryId { get; internal set; }

        /// <inheritdoc />
        public bool IsPublished { get; set; } = true;

        /// <summary>
        /// If this entity is published and there is a draft of it, then it can be navigated through DraftEntity
        /// </summary>
        [PrivateApi]
        internal IEntity DraftEntity { get; set; }

        /// <summary>
        /// If this entity is draft and there is a published edition, then it can be navigated through PublishedEntity
        /// </summary>
        internal IEntity PublishedEntity { get; set; }

        internal int? PublishedEntityId { get; set; } = null;

        /// <inheritdoc />
        public int Version { get; internal set; } = 1;


        #region GetDraft and GetPublished

        /// <inheritdoc />
        public IEntity GetDraft() => DraftEntity;

        /// <inheritdoc />
        public IEntity GetPublished() => PublishedEntity;

        #endregion
    }
}
