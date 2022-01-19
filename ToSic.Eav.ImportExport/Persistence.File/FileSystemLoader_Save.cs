using ToSic.Eav.Data;
using ToSic.Eav.ImportExport;
using ToSic.Eav.ImportExport.Validation;
using ToSic.Eav.Interfaces;
using IEntity = ToSic.Eav.Data.IEntity;

// ReSharper disable once CheckNamespace
namespace ToSic.Eav.Persistence.File
{
    public partial class FileSystemLoader : IRepositorySaver
    {
        public const int QueryMetadataDepth = 10;

        public void SaveContentType(IContentType type)
        {
            var json = Serializer.Serialize(type);

            var cleanName = (type.Scope + "." + type.NameId)
                .RemoveNonFilenameCharacters();

            System.IO.Directory.CreateDirectory(ContentTypePath);

            var fileName = ContentTypePath + cleanName + ImpExpConstants.Extension(ImpExpConstants.Files.json);

            System.IO.File.WriteAllText(fileName, json);
        }

        public void SaveQuery(IEntity queryDef)
        {
            var json = Serializer.Serialize(queryDef, QueryMetadataDepth);

            var cleanname = queryDef.EntityGuid.ToString()
                .RemoveNonFilenameCharacters();

            System.IO.Directory.CreateDirectory(QueryPath);
            var fileName = QueryPath + cleanname + ImpExpConstants.Extension(ImpExpConstants.Files.json);

            System.IO.File.WriteAllText(fileName, json);
       }

    }
}
