﻿using ToSic.Eav.ImportExport.Sys;


// ReSharper disable once CheckNamespace
namespace ToSic.Eav.Persistence.File;

partial class FileSystemLoader
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

        var cleanName = queryDef.EntityGuid.ToString()
            .RemoveNonFilenameCharacters();
        var queryPath = System.IO.Path.Combine(Options.Path, AppDataFoldersConstants.QueriesFolder);
        System.IO.Directory.CreateDirectory(queryPath);
        var fileName = System.IO.Path.Combine(queryPath, cleanName + ImpExpConstants.Extension(ImpExpConstants.Files.json));

        System.IO.File.WriteAllText(fileName, json);
    }

}