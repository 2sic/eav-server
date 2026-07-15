using System.Xml.Linq;
using ToSic.Eav.Apps.Sys;
using ToSic.Eav.ImportExport.Sys.Xml;
using ToSic.Eav.Sys;

namespace ToSic.Eav.ImportExport.Sys;

/// <summary>
/// Reports path casing risks without changing references or files.
/// </summary>
[ShowApiWhenReleased(ShowApiMode.Never)]
public class PathCasePreflightValidator(ILog? parentLog) : HelperBase(parentLog, "Path.Case")
{
    public const string ScopeAppFiles = "AppFiles";
    public const string ScopeSharedAppFiles = "SharedAppFiles";
    public const string ScopeDatabaseAssets = "DatabaseAssets";
    public const string ScopePackageStructure = "PackageStructure";

    public const string CaseMismatch = nameof(CaseMismatch);
    public const string Missing = nameof(Missing);
    public const string FileCollision = nameof(FileCollision);
    public const string FolderCollision = nameof(FolderCollision);

    public PathCasePreflightResult Validate(
        string scope,
        IEnumerable<PathCaseItem> references,
        IEnumerable<PathCaseItem> actualPaths)
    {
        var normalizedActual = Normalize(actualPaths).ToList();
        var actual = normalizedActual
            .Concat(normalizedActual.SelectMany(Parents))
            .Distinct()
            .ToList();
        var actualExact = actual.ToDictionary(Key, StringComparer.Ordinal);
        var actualIgnoreCase = actual.ToLookup(Key, StringComparer.OrdinalIgnoreCase);

        var collisions = actual
            .GroupBy(Key, StringComparer.OrdinalIgnoreCase)
            .Select(group => new
            {
                group.First().IsFolder,
                Paths = group
                    .Select(item => item.Path)
                    .Distinct(StringComparer.Ordinal)
                    .OrderBy(path => path, StringComparer.Ordinal)
                    .ToList()
            })
            .Where(group => group.Paths.Count > 1)
            .Select(group => new PathCaseIssue(
                group.IsFolder ? FolderCollision : FileCollision,
                scope,
                Reference: null,
                group.Paths));

        var referenceIssues = Normalize(references)
            .Where(reference => !actualExact.ContainsKey(Key(reference)))
            .Select(reference =>
            {
                var matches = actualIgnoreCase[Key(reference)]
                    .Select(item => item.Path)
                    .Distinct(StringComparer.Ordinal)
                    .OrderBy(path => path, StringComparer.Ordinal)
                    .ToList();
                return new PathCaseIssue(
                    matches.Count == 0 ? Missing : CaseMismatch,
                    scope,
                    reference.Path,
                    matches);
            });

        return new([.. collisions, .. referenceIssues]);
    }

    public PathCasePreflightResult ValidateImportPackage(string appDirectory, XDocument xml, bool pendingApp)
    {
        var (appReferences, sharedAppReferences) = ViewReferences(xml);
        var assetReferences = xml.Root?
            .Element(XmlConstants.PortalFiles)?
            .Elements(XmlConstants.FileNode)
            .Select(file => file.Attribute(XmlConstants.FolderNodePath)?.Value)
            .Where(path => path.HasValue())
            .Select(path => new PathCaseItem(path!))
            .ToList() ?? [];

        var appFilesRelativeRoot = pendingApp ? string.Empty : FolderConstants.ZipFolderForAppStuff;
        var sharedAppFilesRelativeRoot = pendingApp
            ? Path.Combine(FolderConstants.DataFolderProtected, FolderConstants.ZipFolderForGlobalAppStuff)
            : FolderConstants.ZipFolderForGlobalAppStuff;
        var assetsRelativeRoot = pendingApp
            ? Path.Combine(FolderConstants.DataFolderProtected, FolderConstants.ZipFolderForSiteFiles)
            : FolderConstants.ZipFolderForPortalFiles;

        var expectedRoots = new[]
            {
                (Root: appFilesRelativeRoot, Required: appReferences.Count != 0),
                (Root: sharedAppFilesRelativeRoot, Required: sharedAppReferences.Count != 0)
            }
            .Where(item => item.Root.HasValue()
                && (item.Required || FindActualPaths(Path.Combine(appDirectory, item.Root)).Any(Directory.Exists)))
            .Select(item => item.Root)
            .Concat(assetReferences.Count == 0 ? [] : [assetsRelativeRoot])
            .ToList();

        var actualAppPaths = ActualPaths(Path.Combine(appDirectory, appFilesRelativeRoot));
        if (pendingApp)
            actualAppPaths = actualAppPaths.Where(item => !IsInRoot(item.Path, FolderConstants.DataFolderProtected));

        var results = new List<PathCasePreflightResult>
        {
            Validate(
                ScopePackageStructure,
                expectedRoots.Select(root => new PathCaseItem(root, IsFolder: true)),
                expectedRoots.SelectMany(root => FindActualPaths(Path.Combine(appDirectory, root)))
                    .Where(Directory.Exists)
                    .Select(path => new PathCaseItem(RelativeToRoot(appDirectory, path), IsFolder: true))),
            Validate(
                ScopeAppFiles,
                appReferences,
                actualAppPaths),
            Validate(
                ScopeSharedAppFiles,
                sharedAppReferences,
                ActualPaths(Path.Combine(appDirectory, sharedAppFilesRelativeRoot))),
            Validate(
                ScopeDatabaseAssets,
                assetReferences,
                ActualPaths(Path.Combine(appDirectory, assetsRelativeRoot)))
        };

        return new(results.SelectMany(result => result.Issues).ToList());
    }

    private static (IReadOnlyList<PathCaseItem> App, IReadOnlyList<PathCaseItem> Shared) ViewReferences(XDocument xml)
    {
        var views = xml.Root?
            .Element(XmlConstants.Entities)?
            .Elements(XmlConstants.Entity)
            .Where(entity => entity.Attribute(XmlConstants.AttSetStatic)?.Value == AppConstants.TemplateContentType)
            .ToList() ?? [];

        IReadOnlyList<PathCaseItem> Paths(bool shared) => views
            .Where(view => IsSharedView(view) == shared)
            .SelectMany(view => view.Elements(XmlConstants.ValueNode))
            .Where(value => value.Attribute(XmlConstants.KeyAttr)?.Value == ViewPathKey)
            .Select(value => value.Attribute(XmlConstants.ValueAttr)?.Value)
            .Where(path => path.HasValue())
            .Select(path => new PathCaseItem(path!))
            .ToList();

        return (Paths(shared: false), Paths(shared: true));
    }

    private static bool IsSharedView(XElement view)
    {
        var location = view.Elements(XmlConstants.ValueNode)
            .FirstOrDefault(value => value.Attribute(XmlConstants.KeyAttr)?.Value == ViewLocationKey)
            ?.Attribute(XmlConstants.ValueAttr)?.Value;
        return location.EqualsInsensitive(ViewLocationGlobal)
            || location.EqualsInsensitive(ViewLocationGlobalOld);
    }

    public PathCasePreflightResult LogResult(PathCasePreflightResult result)
    {
        var l = Log.Fn<PathCasePreflightResult>($"{result.Issues.Count} issues");

        if (!result.IsValid)
            foreach (var issue in result.Issues)
                l.W($"{nameof(PathCasePreflightValidator)} - {issue.Type}; scope:'{issue.Scope}'; reference:'{issue.Reference ?? "-"}'; actual:'{string.Join(" | ", issue.ActualPaths)}'");

        return l.Return(result, result.IsValid ? "no issues" : $"{result.Issues.Count} issues");
    }

    public static IReadOnlyList<string> FindActualPaths(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return [];

        var fullPath = Path.GetFullPath(path);
        var root = Path.GetPathRoot(fullPath);
        if (string.IsNullOrEmpty(root))
            return [];

        var segments = fullPath.Substring(root.Length)
            .Split([Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar], StringSplitOptions.RemoveEmptyEntries);
        IReadOnlyList<string> current = [root];
        foreach (var segment in segments)
        {
            current = current
                .Where(Directory.Exists)
                .SelectMany(Directory.EnumerateFileSystemEntries)
                .Where(entry => Path.GetFileName(entry).Equals(segment, StringComparison.OrdinalIgnoreCase))
                .ToList();
            if (current.Count == 0)
                return [];
        }

        return current;
    }

    private static IEnumerable<PathCaseItem> ActualPaths(string root)
        => FindActualPaths(root)
            .Where(Directory.Exists)
            .SelectMany(actualRoot => Directory.EnumerateFiles(actualRoot, "*", SearchOption.AllDirectories)
                .Select(path => new PathCaseItem(RelativeToRoot(actualRoot, path)))
                .Concat(Directory.EnumerateDirectories(actualRoot, "*", SearchOption.AllDirectories)
                    .Select(path => new PathCaseItem(RelativeToRoot(actualRoot, path), IsFolder: true))));

    private static string RelativeToRoot(string root, string path)
        => path.Substring(root.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar).Length)
            .TrimPrefixSlash()
            .ForwardSlash();

    private static bool IsInRoot(string path, string root)
        => path.Equals(root, StringComparison.OrdinalIgnoreCase)
            || path.StartsWith(root + "/", StringComparison.OrdinalIgnoreCase);

    private static IEnumerable<PathCaseItem> Normalize(IEnumerable<PathCaseItem> items)
        => items
            .Select(item => item with { Path = Normalize(item.Path) })
            .Where(item => item.Path.HasValue())
            .Distinct();

    private static string Normalize(string? path)
        => path.ForwardSlash().TrimPrefixSlash().TrimLastSlash() ?? string.Empty;

    private static IEnumerable<PathCaseItem> Parents(PathCaseItem item)
    {
        var parent = item.Path;
        while (parent.LastIndexOf('/') is var separator && separator > 0)
        {
            parent = parent.Substring(0, separator);
            yield return new(parent, IsFolder: true);
        }
    }

    private static string Key(PathCaseItem item)
        => $"{(item.IsFolder ? 'D' : 'F')}:{item.Path}";

    private const string ViewPathKey = "Path";
    private const string ViewLocationKey = "Location";
    private const string ViewLocationGlobal = "Global";
    private const string ViewLocationGlobalOld = "Host File System";
}

[ShowApiWhenReleased(ShowApiMode.Never)]
public sealed record PathCaseItem(string Path, bool IsFolder = false);

[ShowApiWhenReleased(ShowApiMode.Never)]
public sealed record PathCaseIssue(string Type, string Scope, string? Reference, IReadOnlyList<string> ActualPaths);

[ShowApiWhenReleased(ShowApiMode.Never)]
public sealed record PathCasePreflightResult(IReadOnlyList<PathCaseIssue> Issues)
{
    public bool IsValid => Issues.Count == 0;
}
