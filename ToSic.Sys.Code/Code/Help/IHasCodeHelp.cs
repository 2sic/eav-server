namespace ToSic.Sys.Code.Help;

/// <summary>
/// TODO: This actually doesn't really work as expected.
/// Reason is that most code errors appear before compiling works, so the object can't be accessed for the helpers :(
/// must refactor
/// </summary>
[ShowApiWhenReleased(ShowApiMode.Never)]
public interface IHasCodeHelp
{
    List<CodeHelp> ErrorHelpers { get; }
}