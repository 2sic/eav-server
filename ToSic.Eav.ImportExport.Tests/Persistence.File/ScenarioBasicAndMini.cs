namespace ToSic.Eav.ImportExport.Tests.Persistence.File;

/// <summary>
/// Basic setup with all the defaults and no special features / licenses activated.
/// It will also prepare the AdditionalGlobalFolderRepository to also load the mini.
/// </summary>
public record ScenarioBasicAndMini: ScenarioBasic
{
    public ScenarioBasicAndMini()
    {
        AdditionalGlobalFolderRepositoryForReflection.PathToUse =
            TestFiles.GetTestPath(PersistenceTestConstants.ScenarioMini);
    }
}