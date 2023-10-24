using System.Collections.Generic;
using System.Diagnostics;
using ToSic.Eav.Data;
using ToSic.Eav.Persistence.File;
using ToSic.Eav.Repositories;
using ToSic.Lib.Logging;

namespace ToSic.Eav.ImportExport.Tests.Persistence.File
{
    public class FileLoaderCtBase: PersistenceTestsBase
    {


        protected IList<IContentType> LoadAllTypes()
        {
            var loader = GetService<FileSystemLoader>()
                .Init(Constants.PresetAppId, TestStorageRoot, RepositoryTypes.TestingDoNotUse, false, null);
            IList<IContentType> cts;
            try
            {
                cts = loader.ContentTypes();
            }
            finally
            {
                Trace.Write(Log.Dump());
            }
            return cts;
        }
    }
}
