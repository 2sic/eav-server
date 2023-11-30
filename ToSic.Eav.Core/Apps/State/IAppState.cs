using System.Collections.Generic;
using System.Collections.Immutable;
using ToSic.Eav.Data;
using ToSic.Eav.Metadata;
using ToSic.Lib.Data;
using ToSic.Sxc.Apps;

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.Apps
{
    public interface IAppState: IAppIdentity, IMetadataSource, IHasIdentityNameId
    {
        #region Basic App Properties

        string Name { get; }

        string Folder { get; }

        #endregion

        #region Advanced Properties

        IAppConfiguration Configuration { get; }

        #endregion

        #region Content Types and Data (List)

        IImmutableList<IEntity> List { get; }

        IEntity GetDraft(IEntity entity);

        IEntity GetPublished(IEntity entity);


        IEnumerable<IContentType> ContentTypes { get; }

        IContentType GetContentType(string name);

        IContentType GetContentType(int contentTypeId);

        #endregion

        #region Metadata

        public IMetadataOf Metadata { get; }

        #endregion

    }
}
