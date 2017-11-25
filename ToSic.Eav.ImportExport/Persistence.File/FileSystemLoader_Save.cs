using System;
using ToSic.Eav.ImportExport;
using ToSic.Eav.ImportExport.Validation;
using ToSic.Eav.Interfaces;

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.Persistence.File
{
    public partial class FileSystemLoader : IRepositorySaver
    {
        public void SaveContentType(IContentType type)
        {
            var json = Serializer.Serialize(type);

            var cleanName = type.Scope + "." + type.StaticName;
            cleanName = cleanName.RemoveNonFilenameCharacters();

            System.IO.Directory.CreateDirectory(ContentTypePath);

            var fileName = ContentTypePath + cleanName + ImpExpConstants.Extension(ImpExpConstants.Files.json);

            System.IO.File.WriteAllText(fileName, json);
        }

        public void SaveEntity(IEntity item)
        {
            throw new NotImplementedException("save entity not provided yet");
        }
    }
}
