using System.Collections.Generic;

namespace ToSic.Eav.Apps;

public interface IAppJsonService
{
    void MoveAppJsonTemplateFromOldToNewLocation();

    string GetPathToDotAppJson(int appId);

    string GetPathToDotAppJson(string sourceFolder);

    string GetDotAppJson(int appId);

    List<string> ExcludeSearchPatterns(string sourceFolder);
}