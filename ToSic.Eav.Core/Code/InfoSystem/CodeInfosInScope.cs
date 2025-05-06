using ToSic.Eav.Code.Infos;
using ToSic.Eav.Plumbing;
using ToSic.Lib.Helpers;
using ToSic.Lib.Internal.Generics;

namespace ToSic.Eav.Code.InfoSystem;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class CodeInfosInScope(CodeInfoStats codeInfoStats)
{
    public readonly CodeInfoStats CodeInfoStats = codeInfoStats;

    public IEnumerable<CodeInfoInLogStore> GetObsoletes() => Warnings?.Where(x => x.Use.Change.Type == CodeInfoTypes.Obsolete) ?? new List<CodeInfoInLogStore>();
    public IEnumerable<CodeInfoInLogStore> GetWarnings() => Warnings?.Where(x => x.Use.Change.Type == CodeInfoTypes.Warning) ?? new List<CodeInfoInLogStore>();

    private IEnumerable<CodeInfoInLogStore> Warnings => _warnings;
    private readonly List<CodeInfoInLogStore> _warnings = [];

    /// <summary>
    /// Add it to the list and ensure that any known specs are also included
    /// </summary>
    /// <param name="codeInfoUse"></param>
    internal void AddObsolete(CodeInfoInLogStore codeInfoUse)
    {
        if (codeInfoUse == null) return;
        codeInfoUse.EntryOrNull?.UpdateSpecs(Specs);
        _warnings.Add(codeInfoUse);
        CodeInfoStats.Register(codeInfoUse.EntryOrNull);
    }

    /// <summary>
    /// Add context information and update anything that was previously added
    /// </summary>
    public void AddContext(Func<IDictionary<string, string>> specsFactory, string entryPoint = default)
    {
        if (entryPoint != null) EntryPoint = entryPoint;
        if (specsFactory == null) return;
        _specsFactory = specsFactory;
        _specs.Reset();

        // If nothing to add, ignore.
        if (!_warnings.SafeAny()) return;

        // We could use some specs, so let's get them
        var specs = Specs;
        foreach (var logged in _warnings)
        {
            logged?.EntryOrNull?.UpdateSpecs(specs);
            // re-check registration, now that the specs may have changed
            CodeInfoStats.Register(logged?.EntryOrNull);
        }
    }
    internal string EntryPoint { get; private set; }

    private IDictionary<string, string> Specs => _specs.Get(() =>
    {
        try
        {
            return _specsFactory?.Invoke();
        }
        catch
        {
            return null;
        }
    });


    private readonly GetOnce<IDictionary<string, string>> _specs = new();
    private Func<IDictionary<string, string>> _specsFactory;
}