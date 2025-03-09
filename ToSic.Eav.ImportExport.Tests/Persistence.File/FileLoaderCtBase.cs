using System.Collections.Generic;
using System.Diagnostics;
using ToSic.Eav.Data;
using ToSic.Eav.Persistence.File;
using ToSic.Eav.Plumbing;
using ToSic.Eav.Repositories;
using ToSic.Lib.Logging;

namespace ToSic.Eav.ImportExport.Tests.Persistence.File;

public class FileLoaderCtBase: PersistenceTestsBase
{


    protected IList<IContentType> LoadAllTypes(string subfolderOnly = default)
    {
        // Log the root for debugging in case files are missing
        var testRoot = TestStorageRoot;
        if (subfolderOnly.HasValue())
            testRoot += subfolderOnly + '\\';
        Trace.WriteLine("Test folder: " + testRoot);

        var loader = GetService<FileSystemLoader>()
            .Init(Constants.PresetAppId, testRoot, RepositoryTypes.TestingDoNotUse, subfolderOnly != default, null);
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