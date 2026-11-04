using ToSic.Eav.ImportExport.Sys;


// ReSharper disable once CheckNamespace
namespace ToSic.Eav.Persistence.File;

partial class FileSystemLoader
{
    

    public void SaveContentType(IContentType type)
    {
        var json = jsonWriter.New().Serialize(type);

        var cleanName = (type.Scope + "." + type.NameId)
            .RemoveNonFilenameCharacters();

        Directory.CreateDirectory(ContentTypePath);

        var fileName = Path.Combine(ContentTypePath, cleanName + ImpExpConstants.Extension(ImpExpConstants.Files.json));

        System.IO.File.WriteAllText(fileName, json);
    }

    public void SaveQuery(IEntity queryDef)
    {
        var json = jsonWriter.New().Serialize(queryDef, FileSystemLoaderConstants.QueryMetadataDepth);

        var cleanName = queryDef.EntityGuid.ToString()
            .RemoveNonFilenameCharacters();
        var queryPath = Path.Combine(MyOptions.Path, AppDataFoldersConstants.QueriesFolder);
        Directory.CreateDirectory(queryPath);
        var fileName = Path.Combine(queryPath, cleanName + ImpExpConstants.Extension(ImpExpConstants.Files.json));

        System.IO.File.WriteAllText(fileName, json);
    }

}