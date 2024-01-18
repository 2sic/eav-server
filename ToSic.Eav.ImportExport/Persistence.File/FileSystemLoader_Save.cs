using ToSic.Eav.ImportExport;
using ToSic.Eav.ImportExport.Internal;
using ToSic.Eav.ImportExport.Validation;
using ToSic.Eav.Internal.Loaders;
using ToSic.Eav.Repositories;
using IEntity = ToSic.Eav.Data.IEntity;

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.Persistence.File;

partial class FileSystemLoader : IRepositorySaver
{
    

    public void SaveContentType(IContentType type)
    {
        var json = Serializer.Serialize(type);

        var cleanName = (type.Scope + "." + type.NameId)
            .RemoveNonFilenameCharacters();

        System.IO.Directory.CreateDirectory(ContentTypePath);

        var fileName = System.IO.Path.Combine(ContentTypePath, cleanName + ImpExpConstants.Extension(ImpExpConstants.Files.json));

        System.IO.File.WriteAllText(fileName, json);
    }

    public void SaveQuery(IEntity queryDef)
    {
        var json = Serializer.Serialize(queryDef, FileSystemLoaderConstants.QueryMetadataDepth);

        var cleanname = queryDef.EntityGuid.ToString()
            .RemoveNonFilenameCharacters();
        var queryPath = System.IO.Path.Combine(Path, FsDataConstants.QueriesFolder);
        System.IO.Directory.CreateDirectory(queryPath);
        var fileName = System.IO.Path.Combine(queryPath, cleanname + ImpExpConstants.Extension(ImpExpConstants.Files.json));

        System.IO.File.WriteAllText(fileName, json);
    }

}