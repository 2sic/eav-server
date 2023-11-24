using ToSic.Eav.Repository.Efc.Parts;

namespace ToSic.Eav.Repository.Efc;

partial class DbDataController
{
    // Extracted, now externalized objects with actions and private fields

    internal DbVersioning Versioning => _versioning ??= new DbVersioning(this, _compressor);
    private DbVersioning _versioning;
    internal DbEntity Entities => _entities ??= new DbEntity(this, _builder);
    private DbEntity _entities;
    internal DbValue Values => _values ??= new DbValue(this);
    private DbValue _values;
    internal DbAttribute Attributes => _attributes ??= new DbAttribute(this);
    private DbAttribute _attributes;
    internal DbRelationship Relationships => _relationships ??= new DbRelationship(this);
    private DbRelationship _relationships;
    internal DbAttributeSet AttribSet => _attributeSet ??= new DbAttributeSet(this);
    private DbAttributeSet _attributeSet;
    internal DbPublishing Publishing => _publishing ??= new DbPublishing(this, _builder);
    private DbPublishing _publishing;
    internal DbDimensions Dimensions => _dimensions ??= new DbDimensions(this);
    private DbDimensions _dimensions;
    internal DbZone Zone => _dbZone ??= new DbZone(this);
    private DbZone _dbZone;
    internal DbApp App => _dbApp ??= new DbApp(this);
    private DbApp _dbApp;
    internal DbContentType ContentType => _contentType ??= new DbContentType(this);
    private DbContentType _contentType;
}