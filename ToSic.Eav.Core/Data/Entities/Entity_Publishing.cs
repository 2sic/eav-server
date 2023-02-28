using ToSic.Lib.Documentation;

namespace ToSic.Eav.Data
{
    public partial class Entity
    {
        /// <inheritdoc />
        [PrivateApi]
        public int RepositoryId { get; }

        #region Save/Update settings - needed when passing this object to the save-layer

        /// <inheritdoc />
        // TODO: should move the set-info to a save-options object
        public bool IsPublished { get; set; } = true;

        internal int? PublishedEntityId { get; }

        // todo: move to save options
        [PrivateApi]
        public bool PlaceDraftInBranch { get; set; }

        #endregion

        /// <summary>
        /// If this entity is published and there is a draft of it, then it can be navigated through DraftEntity
        /// </summary>
        [PrivateApi]
        internal IEntity DraftEntity { get; set; }

        /// <summary>
        /// If this entity is draft and there is a published edition, then it can be navigated through PublishedEntity
        /// </summary>
        [PrivateApi]
        internal IEntity PublishedEntity { get; set; }


        /// <inheritdoc />
        public int Version { get; }


        #region GetDraft and GetPublished - TODO: must remove this some day soon, they don't fit the clean immutable concept

        /// <inheritdoc />
        public IEntity GetDraft() => DraftEntity;

        /// <inheritdoc />
        public IEntity GetPublished() => PublishedEntity;

        #endregion
    }
}
