using ToSic.Eav.Repository.Efc.Parts;

namespace ToSic.Eav.Repository.Efc;

partial class DbDataController
{
    // Extracted, now externalized objects with actions and private fields

    internal DbVersioning Versioning => _versioning ??= new(this, _compressor);
    private DbVersioning _versioning;
    internal DbEntity Entities => _entities ??= new(this, _builder);
    private DbEntity _entities;
    internal DbValue Values => _values ??= new(this);
    private DbValue _values;
    internal DbAttribute Attributes => _attributes ??= new(this);
    private DbAttribute _attributes;
    internal DbRelationship Relationships => _relationships ??= new(this);
    private DbRelationship _relationships;
    internal DbAttributeSet AttribSet => _attributeSet ??= new(this);
    private DbAttributeSet _attributeSet;
    internal DbPublishing Publishing => _publishing ??= new(this, _builder);
    private DbPublishing _publishing;
    internal DbDimensions Dimensions => _dimensions ??= new(this);
    private DbDimensions _dimensions;
    internal DbZone Zone => _dbZone ??= new(this);
    private DbZone _dbZone;
    internal DbApp App => _dbApp ??= new(this);
    private DbApp _dbApp;
    internal DbContentType ContentType => _contentType ??= new(this);
    private DbContentType _contentType;
}