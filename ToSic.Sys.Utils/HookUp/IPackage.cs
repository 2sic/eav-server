namespace ToSic.Sys.HookUp;

/// <summary>
/// Package without generic type - not sure if we need / keep this.
/// </summary>
public interface IPackage
{
    DataPreprocessorDecision Decision { get; init; }
    List<Exception> Exceptions { get; init; }
}