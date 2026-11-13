using ToSic.Eav.Data.Sys.Entities;
using ToSic.Eav.Persistence.File;
using ToSic.Eav.Serialization;
using ToSic.Eav.WebApi.Sys.Security;
using ToSic.Sys.Users;

namespace ToSic.Eav.ImportExport.Sys.JsonData;

[ShowApiWhenReleased(ShowApiMode.Never)]
public class ExportEntityJson(Generator<IJsonDataWriter> jsonSerializer)
    : ServiceWithSetup<IAppWorkContext>("Api.EaCtEx", connect: [jsonSerializer])
{

    public FileResult DownloadEntityAsJson(IUser user, int id, string prefix, bool withMetadata)
    {
        var l = Log.Fn<FileResult>($"get fields id:{id}");
        SecurityHelpers.ThrowIfNotSiteAdmin(user, l);
        var entity = MyOptions.AppReader.List.FindRepoId(id)
                     ?? throw new($"Can't find entity with id {id}");
        var serializer = jsonSerializer.New();

        return l.ReturnAsOk(new()
        {
            FileName = (prefix + (string.IsNullOrWhiteSpace(prefix) ? "" : ".")
                               + entity.GetBestTitle() + ImpExpConstants.Extension(ImpExpConstants.Files.json))
                .RemoveNonFilenameCharacters(),
            ContentType = MimeTypeConstants.Json,
            FileBytes = System.Text.Encoding.UTF8.GetBytes(serializer.Serialize(entity,
                withMetadata ? FileSystemLoaderConstants.QueryMetadataDepth : 0))
        });

    }
}
