﻿namespace ToSic.Eav.Testing.Scenarios;

/// <summary>
/// Advanced Patrons setup with additional licenses activated etc.
/// Requires that the developer has access to the dev-materials repository.
/// </summary>
public record ScenarioFullPatronsWithDb : TestScenario
{
    public override string ConStr => ScenarioConstants.DefaultConnectionString;
    public override string GlobalFolder => ScenarioConstants.DefaultGlobalFolder;
    public override string GlobalDataCustomFolder => $"{ScenarioConstants.DevMaterialsRoot}{ScenarioConstants.DevMaterialsEnd}";
}

public record ScenarioFullPatronsNoDb : ScenarioFullPatronsWithDb
{
    public override bool UseDb => false;
}