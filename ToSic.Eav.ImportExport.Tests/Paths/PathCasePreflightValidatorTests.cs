using System.Xml.Linq;
using ToSic.Eav.Apps.Sys;
using ToSic.Eav.ImportExport.Sys;
using ToSic.Eav.ImportExport.Sys.Xml;
using ToSic.Eav.Sys;

namespace ToSic.Eav.ImportExport.Tests.Paths;

public class PathCasePreflightValidatorTests
{
    private readonly PathCasePreflightValidator _validator = new(parentLog: null);

    [Fact]
    public void Validate_ReportsCaseMismatch()
    {
        var result = _validator.Validate("App", [new("images/logo.png")], [new("Images/Logo.png")]);

        var issue = Single(result.Issues);
        Equal(PathCasePreflightValidator.CaseMismatch, issue.Type);
        Equal("Images/Logo.png", Single(issue.ActualPaths));
    }

    [Fact]
    public void Validate_ReportsMissingPath()
    {
        var result = _validator.Validate("App", [new("missing.png")], []);

        Equal(PathCasePreflightValidator.Missing, Single(result.Issues).Type);
    }

    [Fact]
    public void Validate_ReportsFileAndFolderCollisions()
    {
        var result = _validator.Validate("App", [], [new("Images/Logo.png"), new("images/logo.png")]);

        Equal(
            [PathCasePreflightValidator.FileCollision, PathCasePreflightValidator.FolderCollision],
            result.Issues.Select(issue => issue.Type).OrderBy(type => type));
    }

    [Fact]
    public void Validate_ExactPathHasNoIssues()
    {
        var result = _validator.Validate("App", [new(@"Images\Logo.png")], [new("Images/Logo.png")]);

        True(result.IsValid);
    }

    [Fact]
    public void LogResult_WritesIssueDetails_WhenParentIsLogCall()
    {
        var parent = new Log("Tst.Parent");
        var call = parent.Fn();
        var validator = new PathCasePreflightValidator(call);
        var result = validator.Validate("App", [new("images/logo.png")], [new("Images/Logo.png")]);

        validator.LogResult(result);
        call.Done();

        Contains(parent.Entries, entry => entry.Message ==
            $"{LogConstants.WarningPrefix}{nameof(PathCasePreflightValidator)} - {PathCasePreflightValidator.CaseMismatch}; scope:'App'; reference:'images/logo.png'; actual:'Images/Logo.png'");
    }

    [Fact]
    public void FindActualPaths_ReturnsDiskCasing()
    {
        var root = Path.Combine(Path.GetTempPath(), "2sxc-path-case-tests", Guid.NewGuid().ToString("N"));
        var folder = Directory.CreateDirectory(Path.Combine(root, "ActualFolder"));
        var file = Path.Combine(folder.FullName, "ActualFile.txt");
        File.WriteAllText(file, "ok");

        try
        {
            var result = PathCasePreflightValidator.FindActualPaths(Path.Combine(root, "actualfolder", "actualfile.TXT"));

            Equal(file, Single(result));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void ValidateImportPackage_ReportsXmlAssetCaseMismatch()
    {
        var root = Path.Combine(Path.GetTempPath(), "2sxc-path-case-tests", Guid.NewGuid().ToString("N"));
        var portalFiles = Directory.CreateDirectory(Path.Combine(root, FolderConstants.ZipFolderForPortalFiles, "Adam"));
        File.WriteAllText(Path.Combine(portalFiles.FullName, "ActualFile.png"), "ok");
        var xml = new XDocument(
            new XElement(XmlConstants.RootNode,
                new XElement(XmlConstants.PortalFiles,
                    new XElement(XmlConstants.FileNode,
                        new XAttribute(XmlConstants.FolderNodePath, "adam/actualfile.png")))));

        try
        {
            var result = _validator.ValidateImportPackage(root, xml, pendingApp: false);

            var issue = Single(result.Issues, issue => issue.Scope == PathCasePreflightValidator.ScopeDatabaseAssets);
            Equal(PathCasePreflightValidator.CaseMismatch, issue.Type);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void ValidateImportPackage_ReportsCaseCollidingFiles_OnCaseSensitiveFileSystem()
    {
        var root = Path.Combine(Path.GetTempPath(), "2sxc-path-case-tests", Guid.NewGuid().ToString("N"));
        var appFiles = Directory.CreateDirectory(Path.Combine(root, FolderConstants.ZipFolderForAppStuff));
        File.WriteAllText(Path.Combine(appFiles.FullName, "File.txt"), "first");
        File.WriteAllText(Path.Combine(appFiles.FullName, "file.txt"), "second");

        try
        {
            if (Directory.GetFiles(appFiles.FullName).Length < 2)
                return;

            var result = _validator.ValidateImportPackage(
                root,
                new(new XElement(XmlConstants.RootNode)),
                pendingApp: false);

            Contains(result.Issues, issue =>
                issue.Scope == PathCasePreflightValidator.ScopeAppFiles
                && issue.Type == PathCasePreflightValidator.FileCollision);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ValidateImportPackage_ReportsViewPathCaseMismatch(bool pendingApp)
    {
        var root = Path.Combine(Path.GetTempPath(), "2sxc-path-case-tests", Guid.NewGuid().ToString("N"));
        var appRoot = pendingApp ? root : Path.Combine(root, FolderConstants.ZipFolderForAppStuff);
        var views = Directory.CreateDirectory(Path.Combine(appRoot, "Views"));
        File.WriteAllText(Path.Combine(views.FullName, "Actual.cshtml"), "ok");
        var xml = new XDocument(
            new XElement(XmlConstants.RootNode,
                new XElement(XmlConstants.Entities,
                    new XElement(XmlConstants.Entity,
                        new XAttribute(XmlConstants.AttSetStatic, AppConstants.TemplateContentType),
                        new XElement(XmlConstants.ValueNode,
                            new XAttribute(XmlConstants.KeyAttr, "Path"),
                            new XAttribute(XmlConstants.ValueAttr, "views/actual.cshtml"))))));

        try
        {
            var result = _validator.ValidateImportPackage(root, xml, pendingApp);

            var issue = Single(result.Issues, issue => issue.Scope == PathCasePreflightValidator.ScopeAppFiles);
            Equal(PathCasePreflightValidator.CaseMismatch, issue.Type);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }
}
