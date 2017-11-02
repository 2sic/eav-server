using System.Collections.Generic;
using ToSic.Eav.Enums;
using ToSic.Eav.Interfaces;
using ToSic.Eav.Logging;
using ToSic.Eav.Persistence.File;

namespace ToSic.Eav.ImportExport.Persistence.File
{
    public abstract class Runtime : HasLog, IRuntime
    {
        protected Runtime(string logName) : base(logName)
        {

        }

        // 1 - find the current path to the .data folder
        public abstract string Path { get; }

        public Repositories Source { get; set; } = Repositories.SystemFiles;

        public IEnumerable<IContentType> LoadGlobalContentTypes()
        {
            Log.Add("loading types");

            // 3 - return content types
            var types = Loader.ContentTypes(0, null);
            Log.Add($"found {types?.Count} types");
            return types;
        }

        public FileSystemLoader Loader => _loader ?? (_loader = new FileSystemLoader(Path, Source, true, Log));
        private FileSystemLoader _loader;

    }
}