using ToSic.Eav.Apps.Sys.AppJson;
using ToSic.Eav.ImportExport.Sys;
using ToSic.Eav.Persistence.Logging;
using ToSic.Eav.Sys;

namespace ToSic.Eav.ImportExport.Internal;

[ShowApiWhenReleased(ShowApiMode.Never)]
public class AppFileManager(LazySvc<IAppJsonConfigurationService> appJsonService) : ServiceBase(EavLogs.Eav + ".FileMn")
{
    /// <summary>
    /// Separator for file search patterns.
    /// NOTE: not in use as of 2025-01, so it could still be changed if necessary
    /// </summary>
    private const char Separator = ';';

    public AppFileManager SetFolder(int appId, string sourceFolder, string subFolder = null)
    {
        // appId is needed to get app.json from cache
        _appId = appId;
        // source folder needed to get AppRoot for app.json
        _sourceFolder = sourceFolder;
        // we need it as "root" for files for export, usually same or under "sourceFolder"
        _root = string.IsNullOrEmpty(sourceFolder)
                ? string.Empty
                : string.IsNullOrEmpty(subFolder)
                    ? sourceFolder
                    : Path.Combine(sourceFolder, subFolder);
        return this;
    }

    private int _appId;
    private string _sourceFolder;
    private string _root;

    /// <summary>
    /// Copy all files from SourceFolder to DestinationFolder (directly on the file system)
    /// </summary>
    /// <param name="destinationFolder"></param>
    /// <param name="overwriteFiles"></param>
    /// <param name="messages"></param>
    public void CopyAllFiles(string destinationFolder, bool overwriteFiles, List<Message> messages) 
    {
        var l = Log.Fn($"dest:{destinationFolder}, overwrite:{overwriteFiles}");
        var filteredFiles = GetAllTransferableFiles().ToList();
        l.A($"copy files:{filteredFiles.Count}");
        foreach (var file in filteredFiles)
        {
            var relativeFilePath = file.Replace(_root, "").TrimPrefixSlash();
            var destinationFilePath = $"{destinationFolder}{Path.DirectorySeparatorChar}{relativeFilePath}";
            l.A($"relFilePath:{relativeFilePath},destFilePath:{destinationFilePath}");

            Directory.CreateDirectory(Path.GetDirectoryName(destinationFilePath) ?? string.Empty);

            if (!File.Exists(destinationFilePath))
                File.Copy(file, destinationFilePath, overwriteFiles);
            else
            {
                var alreadyExistsMsg = "File '" + Path.GetFileName(destinationFilePath) +
                                       "' not copied because it already exists";
                Log.A($"warning: {alreadyExistsMsg}");
                messages.Add(new(alreadyExistsMsg, Message.MessageTypes.Warning));
            }
        }

        l.Done();
    }
    
    /// <summary>
    /// Gets all files from a folder and subfolder, which fit the import/export filter criteria
    /// </summary>
    /// <returns></returns>
    public IEnumerable<string> GetAllTransferableFiles(string searchPattern = "*.*")
    {
        var l = Log.Fn<IEnumerable<string>>();
        var hardcodedExcludedFolders = GetHardcodedExcludedFolders();
        l.A($"Hardcoded excluded folders count: {hardcodedExcludedFolders.Count}");

        var specs = new FileSearchSpecs
        {
            // Get exclude search patterns from app.json
            ExcludeSearchPatterns = appJsonService.Value.ExcludeSearchPatterns(_sourceFolder, _appId),
            FileSearchPatterns = searchPattern.Split(Separator).ToList()
        };

        var usingAppJson = specs.ExcludeSearchPatterns.Any();
        var files = usingAppJson 
            ? ExcludeFilesBasedOnExcludeInDotAppJson(_root, specs) // based on app.json
            : AllFiles(specs); // old way
        l.A($"Exclude files {(usingAppJson ? $"based on {FolderConstants.AppJson}" : "using old way")}");

        l.A("Process excluding files based on hardcoded exclusions");
        var filteredFiles = files
            .Where(file => !IsFileInExcludedFolder(file, hardcodedExcludedFolders))
            .ToList();

        return l.Return(filteredFiles, "Done filtering files based on exclusion criteria");
    }

    private List<string> GetHardcodedExcludedFolders()
    {
        var l = Log.Fn<List<string>>();
        // add folder slashes to ensure the term is part of a folder, not within a file-name
        var exclAnyFolder = Settings.ExcludeFolders
            .Select(f => $"{Path.DirectorySeparatorChar}{f}{Path.DirectorySeparatorChar}")
            .ToList();

        var exclRootFolders = Settings.ExcludeRootFolders
            .Select(f => $"{Path.Combine(_root, f)}{Path.DirectorySeparatorChar}")
            .ToList();

        var result = exclAnyFolder.Union(exclRootFolders).ToList();
        return l.Return(result, "Ok, building list of hardcoded excluded folders");
    }

    private static bool IsFileInExcludedFolder(string filePath, IEnumerable<string> excludedFolders) 
        => excludedFolders.Any(ex => filePath.ToLowerInvariant().Contains(ex.ToLowerInvariant()));


    public IEnumerable<string> GetAllTransferableFolders()
        => GetAllTransferableFolders(new());

    /// <summary>
    /// Gets all folders from a folder and subfolder, which fit the import/export filter criteria
    /// </summary>
    /// <returns></returns>
    private IEnumerable<string> GetAllTransferableFolders(FileSearchSpecs specs, string searchPattern = "*.*")
    {
        var l = Log.Fn<IEnumerable<string>>();
        if (appJsonService.Value.ExcludeSearchPatterns(_sourceFolder, _appId).Any()) // exclude files based on app.json from v14.08
            return l.Return(ExcludeFoldersBasedOnExcludeInDotAppJson(_root, specs).ToList(), $"ok, exclude folders based on {FolderConstants.AppJson}");

        // old hardcoded way of excluding files
        l.A($"can't find ExcludeSearchPatterns in {FolderConstants.AppJson}, exclude folders on old way");
        // add folder slashes to ensure the term is part of a folder, not within a file-name
        var exclAnyFolder = Settings.ExcludeFolders
            .Select(f => $"{Path.DirectorySeparatorChar}{f}{Path.DirectorySeparatorChar}")
            .ToList();
        var exclRootFolders = Settings.ExcludeRootFolders
            .Select(f => $"{Path.Combine(_root, f)}{Path.DirectorySeparatorChar}").ToList();
        var excFolders = exclAnyFolder.Union(exclRootFolders).ToList();
        Log.A($"hardcoded, excFolders:{excFolders.Count}");

        return l.Return(AllFolders(specs)
            .Where(f => !excFolders.Any(ex => f.ToLowerInvariant().Contains(ex.ToLowerInvariant())))
            .ToList(), "ok, exclude folders on old way");
    }


    /// <summary>
    /// Get all files from a folder, not caring if it will be exported or not
    /// </summary>
    /// <returns></returns>
    public IEnumerable<string> AllFiles(FileSearchSpecs specs = default) =>
        !Directory.Exists(_root)
            ? []
            : ((specs ?? new())
                .FileSearchPatterns
                .SelectMany(s => Directory.EnumerateFiles(_root, s, SearchOption.AllDirectories))
                .ToList());


    /// <summary>
    /// Get all folders from a folder, not caring if it will be exported or not
    /// </summary>
    /// <returns></returns>
    private IEnumerable<string> AllFolders(FileSearchSpecs specs) =>
        !Directory.Exists(_root)
            ? []
            : specs.FolderSearchPatterns
                .SelectMany(s => Directory.EnumerateDirectories(_root, s, SearchOption.AllDirectories))
                .ToList();


    /// <summary>
    /// Exclude files based on 2sxc special exclude array in app.json
    /// </summary>
    /// <param name="root"></param>
    /// <param name="specs"></param>
    /// <returns></returns>
    private IEnumerable<string> ExcludeFilesBasedOnExcludeInDotAppJson(string root, FileSearchSpecs specs)
    {
        var l = Log.Fn<IEnumerable<string>>($"folderPath:{root}");
        // validate folderPath
        if (!Directory.Exists(root))
            return l.Return([], $"warning, can't find root folder path '{root}'");

        // validate exclude search patterns
        if (!specs.ExcludeSearchPatterns.Any())
        {
            var allFiles = specs.FileSearchPatterns
                .SelectMany(s => Directory.EnumerateFiles(root, s, SearchOption.AllDirectories))
                .ToList();
            return l.Return(allFiles, $"warning, can't find 2sxc exclude in '{FolderConstants.AppJson}'");
        }

        // prepare list of files to exclude using simple wildcard patterns
        l.A($"excludeSearchPatterns:{specs.ExcludeSearchPatterns.Count}");

        // *** EXCLUDE FOLDERS & FILES
        var topLevelFiles = GetFiles(root, specs).ToList();
        var subFiles = GetFilesRecursive(root, specs);
        var files = topLevelFiles
            .Union(subFiles)
            .ToList();

        return l.Return(files,"ok");
    }

    /// <summary>
    /// Exclude folders based on 2sxc special exclude array in app.json
    /// </summary>
    /// <param name="root"></param>
    /// <param name="specs"></param>
    /// <returns></returns>
    private IEnumerable<string> ExcludeFoldersBasedOnExcludeInDotAppJson(string root, FileSearchSpecs specs)
    {
        var l = Log.Fn<IEnumerable<string>>($"folderPath:{root}");
        // validate folderPath
        if (!Directory.Exists(root))
            return l.Return([], $"warning, can't find folder path '{_sourceFolder}'");

        // validate exclude search patterns
        if (!specs.ExcludeSearchPatterns.Any())
        {
            var allFolders = specs.FileSearchPatterns
                .SelectMany(s => Directory.EnumerateDirectories(_root, s, SearchOption.AllDirectories))
                .ToList();
            return l.Return(allFolders, $"warning, can't find 2sxc exclude in '{FolderConstants.AppJson}'");
        }

        // prepare list of files to exclude using simple wildcard patterns
        Log.A($"excludeSearchPatterns:{specs.ExcludeSearchPatterns.Count}");

        // *** EXCLUDE FOLDERS & FILES
        var folders = GetFoldersRecursive(root, specs);

        return l.Return(folders,"ok");
    }

    private List<string> GetFilesRecursive(string folderPath, FileSearchSpecs specs)
    {
        var l = Log.Fn<List<string>>($"folderPath:{folderPath}");
        var subFolders = GetFolders(folderPath, specs).ToList();

        var allFiles = subFolders
            .SelectMany(folder =>
            {
                var folderFiles = GetFiles(folder, specs);
                var deeperFiles = GetFilesRecursive(folder, specs);
                return folderFiles.Union(deeperFiles);
            })
            .ToList();

        return l.Return(allFiles, $"{allFiles.Count}");
    }

    private List<string> GetFoldersRecursive(string folderPath, FileSearchSpecs specs)
    {
        var l = Log.Fn<List<string>>($"folderPath:{folderPath}");
        var subFolders = GetFolders(folderPath, specs).ToList();
        var allFolders = subFolders
            .SelectMany<string, string>(folder => [folder, .. GetFoldersRecursive(folderPath, specs)])
            .ToList();

        return l.Return(allFolders, $"{allFolders.Count}");
    }

    private static IEnumerable<string> GetFolders(string folderPath, FileSearchSpecs specs)
    {
        var folders = Directory
            .EnumerateDirectories(folderPath, "*.*", SearchOption.TopDirectoryOnly)
            .ToList();

        return FilterList(folders, specs.ExcludeSearchPatterns);
    }


    private static IEnumerable<string> GetFiles(string folderPath, FileSearchSpecs specs)
    {
        var files = specs.FileSearchPatterns
            .SelectMany(s => Directory.EnumerateFiles(folderPath, s, SearchOption.TopDirectoryOnly))
            .ToList();

        return FilterList(files, specs.ExcludeSearchPatterns);
    }

    /// <summary>
    /// Filter a list of files or folder according to the exclude list.
    /// </summary>
    /// <param name="paths"></param>
    /// <param name="excludeList"></param>
    /// <returns></returns>
    private static List<string> FilterList(List<string> paths, List<string> excludeList) =>
        excludeList.Aggregate(
                paths,
                (current, ignore) => current
                    .Where(f => !f.ToLowerInvariant().Contains(ignore))
                    .ToList()
            )
            .ToList();

    public record FileSearchSpecs
    {
        public List<string> ExcludeSearchPatterns { get; init; } = [];

        public List<string> FileSearchPatterns { get; init; } = ["*.*"];

        public List<string> FolderSearchPatterns { get; init; } = ["*.*"];
    }
}