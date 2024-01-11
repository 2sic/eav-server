using System;
using ToSic.Eav.Data;
using ToSic.Eav.Metadata;

namespace ToSic.Eav.ImportExport;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class ExportDecorator(IEntity entity) : EntityBasedType(entity)
{
    public static string TypeNameId = "32698880-1c2e-41ab-bcfc-420091d3263f";
    public static string ContentTypeName = "SystemExportDecorator";

    public bool IsContentType => Entity.MetadataFor.TargetType == (int) TargetTypes.ContentType;

    public bool IsEntity => Entity.MetadataFor.TargetType == (int)TargetTypes.Entity;

    public string KeyString => Entity.MetadataFor.KeyString;

    public Guid? KeyGuid => Entity.MetadataFor.KeyGuid;

}