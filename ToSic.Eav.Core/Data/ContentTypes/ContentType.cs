using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using ToSic.Eav.Repositories;
using ToSic.Lib.Documentation;
using ToSic.Lib.Helpers;
using static System.StringComparison;

namespace ToSic.Eav.Data
{
    /// <summary>
    /// Represents a ContentType
    /// </summary>
    // Remarks: Before 2021-09 it was marked as PublicApi
    // We should actually make it PrivateApi, but other code references this, so we need to change that to IContentType,
    // Otherwise docs won't generate cross-links as needed
    [PrivateApi("2021-09-30 hidden now, was internal_don't use Always use the interface, not this class")]
    public partial class ContentType : IContentType, IContentTypeShared
    {
        #region simple properties

        /// <inheritdoc />
        public int AppId { get; }

        /// <inheritdoc />
        public string Name { get; }

        /// <inheritdoc />
        [Obsolete("Deprecated in v13, please use NameId instead")]
        public string StaticName => NameId;

        /// <inheritdoc />
        public string NameId { get; }

        // #RemoveContentTypeDescription #2974 - #remove ca. Feb 2023 if all works
        ///// <inheritdoc />
        //[Obsolete("Obsolete in v12, used to contain the description, which is now in the metadata")]
        //public string Description { get; private set; }

        /// <inheritdoc />
        public string Scope { get; }

        /// <inheritdoc />
        public int Id { get; internal set; }

        /// <inheritdoc />
        [Obsolete("Deprecated in V13, please use Id instead.")]
        public int ContentTypeId => Id;

        /// <inheritdoc />
        public IList<IContentTypeAttribute> Attributes { get; }

        /// <inheritdoc />
        public RepositoryTypes RepositoryType { get; internal set; }

        /// <inheritdoc />
        public string RepositoryAddress { get; internal set; } = "";

        /// <inheritdoc />
        public bool IsDynamic { get; }

        #endregion

        /// <inheritdoc />
        public bool Is(string name) => Name.Equals(name, InvariantCultureIgnoreCase) || NameId.Equals(name, InvariantCultureIgnoreCase);
        
        [JsonIgnore]
        [PrivateApi("new 15.04")]
        public string TitleFieldName => _titleFieldName.Get(() => Attributes.FirstOrDefault(a => a.IsTitle)?.Name);
        private readonly GetOnce<string> _titleFieldName = new GetOnce<string>();

        /// <inheritdoc />
        public IContentTypeAttribute this[string fieldName] => Attributes.FirstOrDefault(a => string.Equals(a.Name, fieldName, OrdinalIgnoreCase));


        #region New DynamicChildren Navigation - new in 12.03

        /// <inheritdoc />
        [PrivateApi("WIP 12.03")]
        // Don't cache the result, as it could change during runtime
        public string DynamicChildrenField => Metadata.DetailsOrNull?.DynamicChildrenField;

        #endregion


        #region constructors

        ///// <inheritdoc />
        ///// <summary>
        ///// Initializes a new ContentType - usually when building the cache
        ///// </summary>
        //[PrivateApi]
        //public ContentType(
        //    int appId,
        //    string name,
        //    string nameId,
        //    int typeId,
        //    string scope,

        //    List<IDecorator<IContentType>> decorators,
        //    ContentTypeMetadata ctMetadata,
        //// #RemoveContentTypeDescription #2974 - #remove ca. Feb 2023 if all works
        ////string description = default,

        //    // Reference to original if it inherits something
        //    //int? parentTypeId = default, 
        //    //int configZoneId = default, 
        //    //int configAppId = default,

        //    // This is a shared type
        //    bool alwaysShareConfig = default,

        //    IList<IContentTypeAttribute> attributes = default,


        //    //Func<IHasMetadataSource> metaSourceFinder = default,
        //    bool isDynamic = default
        //    ): this(appId, id: typeId, name: name, nameId: nameId, 
        //    scope: scope, alwaysShareConfiguration: alwaysShareConfig,
        //    attributes: attributes, isDynamic: isDynamic,
        //    ctMetadata: ctMetadata, decorators: decorators)
        //{
        //    // #RemoveContentTypeDescription #2974 - #remove ca. Feb 2023 if all works
        //    //Description = description;

        //    // move to other constructor
        //    //Scope = Scopes.RenameOldScope(scope);
        //    //AlwaysShareConfiguration = alwaysShareConfig;
        //    //IsDynamic = isDynamic;

        //    //Decorators = decorators ?? new List<IDecorator<IContentType>>();

        //    //if (parentTypeId != null)
        //    //    Decorators.Add(new Ancestor<IContentType>(new AppIdentity(configZoneId, configAppId),
        //    //        parentTypeId.Value));

        //    // Metadata
        //    //_metaSourceFinder = metaSourceFinder;


        //}

        /// <summary>
        /// Basic initializer of ContentType class
        /// </summary>
        /// <remarks>
        /// Overload for in-memory entities
        /// </remarks>
        [PrivateApi]
        public ContentType(
            int appId,
            int id,
            string name,
            string nameId = default,
            string scope = default,
            IList<IContentTypeAttribute> attributes = default,
            bool isAlwaysShared = default,
            bool? onSaveSortAttributes = default,
            string onSaveUseParentStaticName = default,
            RepositoryTypes repositoryType = RepositoryTypes.Sql,
            bool isDynamic = default,
            ContentTypeMetadata ctMetadata = default,
            List<IDecorator<IContentType>> decorators = default
            )
        {
            AppId = appId;
            Id = id;

            Name = name;
            NameId = nameId ?? name;

            RepositoryType = repositoryType;

            AlwaysShareConfiguration = isAlwaysShared;

            IsDynamic = isDynamic;

            Decorators = decorators ?? new List<IDecorator<IContentType>>();
            Metadata = ctMetadata ?? new ContentTypeMetadata(NameId, null, Name);

            if (scope != default)
                Scope = Scopes.RenameOldScope(scope);

            if (attributes != null)
                Attributes = attributes;

            if (onSaveSortAttributes != null)
                OnSaveSortAttributes = onSaveSortAttributes.Value;

            if (onSaveUseParentStaticName != default)
                OnSaveUseParentStaticName = onSaveUseParentStaticName;
        }

        #endregion




    }
}