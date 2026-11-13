using ToSic.Eav.Serialization;
using ToSic.Eav.WebApi.Sys.Security;
using ToSic.Sys.Users;

namespace ToSic.Eav.ImportExport.Sys.JsonData;

[ShowApiWhenReleased(ShowApiMode.Never)]
public class ExportTypeJson(Generator<IJsonDataWriter> jsonSerializer)
    : ServiceWithSetup<IAppWorkContext>("Api.EaCtEx", connect: [jsonSerializer])
{

    public FileResult DownloadTypeAsJson(IUser user, string name)
    {
        var l = Log.Fn<FileResult>($"get fields type:{name}");
        SecurityHelpers.ThrowIfNotSiteAdmin(user, l);
        var type = MyOptions.AppReader.GetContentType(name);
        var serializer = jsonSerializer.New();
        var fileName = (type.Scope + "." + type.NameId + ImpExpConstants.Extension(ImpExpConstants.Files.json))
            .RemoveNonFilenameCharacters();

        var typeJson = serializer.Serialize(type, new()
        {
            CtIncludeInherited = false,
            CtAttributeIncludeInheritedMetadata = false
        });
        return l.ReturnAsOk(new()
        {
            FileName = fileName,
            ContentType = MimeTypeConstants.Json,
            FileBytes = System.Text.Encoding.UTF8.GetBytes(typeJson)
        });
    }
}
