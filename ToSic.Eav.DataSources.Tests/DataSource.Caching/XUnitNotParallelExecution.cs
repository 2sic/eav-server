namespace ToSic.Eav.DataSource.Caching;

[CollectionDefinition(Name, DisableParallelization = true)]
public class XUnitNotParallelExecution
{
    public const string Name = "Non-Parallel Collection";
}