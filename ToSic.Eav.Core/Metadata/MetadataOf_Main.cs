using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Data;
using ToSic.Eav.Security;

namespace ToSic.Eav.Metadata
{
    public partial class MetadataOf<T>
    {


        /// <summary>
        /// All "normal" metadata entities - so it hides the system-entities
        /// like permissions. This is the default view of metadata given by an item
        /// </summary>
        private List<IEntity> MetadataWithoutPermissions
        {
            get
            {
                // If necessary, initialize first. Note that it will only add Ids which really exist in the source (the source should be the cache)
                if (_metadataWithoutPermissions == null || RequiresReload())
                    _metadataWithoutPermissions = AllWithHidden
                        .Where(md => !Permission.IsPermission(md))
                        .ToList();
                return _metadataWithoutPermissions;
            }
        }
        private List<IEntity> _metadataWithoutPermissions;

        /// <inheritdoc />
        public IEnumerable<Permission> Permissions
        {
            get
            {
                if (_permissions == null || RequiresReload())
                    _permissions = AllWithHidden
                        .Where(Permission.IsPermission)
                        .Select(e => new Permission(e));
                return _permissions;
            }
        }
        private IEnumerable<Permission> _permissions;



    }
}
