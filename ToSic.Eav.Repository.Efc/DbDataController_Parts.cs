using ToSic.Eav.Repository.Efc.Parts;

namespace ToSic.Eav.Repository.Efc
{
    public partial class DbDataController
    {
        // Extracted, now externalized objects with actions and private fields

        public DbVersioning Versioning => _versioning ?? (_versioning = new DbVersioning(this, _compressor));
        private DbVersioning _versioning;
        public DbEntity Entities => _entities ?? (_entities = new DbEntity(this, _builder));
        private DbEntity _entities;
        public DbValue Values => _values ?? (_values = new DbValue(this));
        private DbValue _values;
        public DbAttribute Attributes => _attributes ?? (_attributes = new DbAttribute(this));
        private DbAttribute _attributes;
        public DbRelationship Relationships => _relationships ?? (_relationships = new DbRelationship(this));
        private DbRelationship _relationships;
        public DbAttributeSet AttribSet => _attributeSet ?? (_attributeSet = new DbAttributeSet(this));
        private DbAttributeSet _attributeSet;
        internal DbPublishing Publishing => _publishing ?? (_publishing = new DbPublishing(this));
        private DbPublishing _publishing;
        public DbDimensions Dimensions => _dimensions ?? (_dimensions = new DbDimensions(this));
        private DbDimensions _dimensions;
        public DbZone Zone => _dbZone ?? (_dbZone = new DbZone(this));
        private DbZone _dbZone;
        public DbApp App => _dbApp ?? (_dbApp = new DbApp(this));
        private DbApp _dbApp;
        public DbContentType ContentType => _contentType ?? (_contentType = new DbContentType(this));
        private DbContentType _contentType;
    }
}
