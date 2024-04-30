using System.Collections.Generic;

namespace ToSic.Eav.Apps.Services;

// todo: @STV MOVE TO Apps.Internal
public interface IAppJsonService
{
    void MoveAppJsonTemplateFromOldToNewLocation();

    //string GetPathToDotAppJson(int appId);

    string GetPathToDotAppJson(string sourceFolder);

    AppJson GetDotAppJson(int appId);

    List<string> ExcludeSearchPatterns(string sourceFolder);

    // TODO: @STV - appId should not be nullable
    bool DnnCompilerAlwaysUseRoslyn(int? appId);
}