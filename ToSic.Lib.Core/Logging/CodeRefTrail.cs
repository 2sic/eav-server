using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using ToSic.Lib.Documentation;

namespace ToSic.Lib.Logging;

/// <summary>
/// Experimental - system to generate a code ref trail.
///
/// Important: this is _not_ functional, so object changes are apply to all instances of this object.
/// </summary>
[PrivateApi("Experimental")]
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class CodeRefTrail
{
    public CodeRefTrail([CallerFilePath] string cPath = default, [CallerMemberName] string cName = default, [CallerLineNumber] int cLine = default)
    {
        CodeRefs.Add(CodeRef.Create(cPath, cName, cLine));
    }

    public CodeRefTrail WithHere([CallerFilePath] string cPath = default, [CallerMemberName] string cName = default, [CallerLineNumber] int cLine = default)
    {
        CodeRefs.Add(CodeRef.Create(cPath, cName, cLine));
        return this;
    }

    public CodeRefTrail AddMessage(string message, int? number = default)
    {
        CodeRefs.Add(CodeRef.Create(CodeRef.Message, message, number ??0));
        return this;
    }

    public List<CodeRef> CodeRefs { get; } = [];

    public override string ToString() => CodeRefs == null ? "" : string.Join("\n", CodeRefs.Select(cr => cr.ToString()));
}