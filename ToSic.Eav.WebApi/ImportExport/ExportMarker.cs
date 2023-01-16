using System;
using ToSic.Eav.Data;
using ToSic.Eav.Metadata;

namespace ToSic.Eav.WebApi.ImportExport
{
    public class ExportMarker : EntityBasedType
    {
        public ExportMarker(IEntity entity) : base(entity)
        {
        }

        public bool IsContentType => Entity.MetadataFor.TargetType == (int) TargetTypes.ContentType;

        public bool IsEntity => Entity.MetadataFor.TargetType == (int)TargetTypes.Entity;

        public string KeyString => Entity.MetadataFor.KeyString;

        public Guid? KeyGuid => Entity.MetadataFor.KeyGuid;

    }
}
