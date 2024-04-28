using System.Collections.Generic;

namespace ToSic.Eav.Apps.Services;

public interface IAppJsonService
{
    void MoveAppJsonTemplateFromOldToNewLocation();

    //string GetPathToDotAppJson(int appId);

    string GetPathToDotAppJson(string sourceFolder);

    AppJson GetDotAppJson(int appId);

    List<string> ExcludeSearchPatterns(string sourceFolder);
}