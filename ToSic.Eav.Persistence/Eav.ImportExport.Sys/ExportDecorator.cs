using ToSic.Eav.Data.EntityBased.Sys;

namespace ToSic.Eav.ImportExport.Sys;

[ShowApiWhenReleased(ShowApiMode.Never)]
public class ExportDecorator(IEntity entity) : EntityBasedType(entity)
{
    public static string TypeNameId = "32698880-1c2e-41ab-bcfc-420091d3263f";
    public static string ContentTypeName = "SystemExportDecorator";

    public bool IsContentType => Entity.MetadataFor.TargetType == (int) TargetTypes.ContentType;

    public bool IsEntity => Entity.MetadataFor.TargetType == (int)TargetTypes.Entity;

    public string KeyString => Entity.MetadataFor.KeyString;

    public Guid? KeyGuid => Entity.MetadataFor.KeyGuid;

}