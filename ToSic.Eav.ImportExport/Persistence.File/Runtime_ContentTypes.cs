using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Apps;
using ToSic.Eav.Configuration;
using ToSic.Eav.Data;
using ToSic.Eav.ImportExport.Json;
using ToSic.Lib.Logging;

namespace ToSic.Eav.Persistence.File
{
    public partial class Runtime
    {
        public List<IContentType> LoadGlobalContentTypes(AppState appState)
        {
            var l = Log.Fn<List<IContentType>>();

            // Set TypeID seed for loader so each loaded type has a unique ID
            var loaderIndex = 1;
            Loaders.ForEach(ldr =>
            {
                ldr.TypeIdSeed = FsDataConstants.GlobalContentTypeMin +
                                 FsDataConstants.GlobalContentTypeSourceSkip * loaderIndex++;
            });
            

            // 3 - return content types
            var delayedContentTypeProvider = new DeferredContentTypeProvider(Log);

            // #SharedFieldDefinition
            var deSerializeSettings = new JsonDeSerializationSettings
            {
                EntityContentTypeProvider = delayedContentTypeProvider,
                CtAttributeMetadataAppState = appState,
            };

            var newTypes = Loaders.SelectMany(ldr =>
            {
                //ldr.Serializer.ContentTypeProvider = delayedContentTypeProvider;
                ldr.Serializer.DeserializationSettings = deSerializeSettings;
                var result = ldr.ContentTypes();
                ldr.Serializer.DeserializationSettings = null;
                //ldr.Serializer.ContentTypeProvider = null;
                return result;
            }).ToList();

            delayedContentTypeProvider.AddTypes(newTypes);

            var types = delayedContentTypeProvider.ProcessSubEntitiesOnTypes();

            return l.Return(types, $"found {types.Count} types");
        }

    }
}
