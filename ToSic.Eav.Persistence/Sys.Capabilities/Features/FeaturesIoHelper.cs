using ToSic.Eav.Sys.Configuration;
using ToSic.Sys.Capabilities;
using ToSic.Sys.Configuration;

namespace ToSic.Eav.Sys.Capabilities.Features;

public class FeaturesIoHelper(IGlobalConfiguration globalConfiguration) : ServiceBase("FeatCfgMng")
{
    /// <summary>
    /// Return content from 'features.json' file.
    /// Also return full path to 'features.json'.
    /// </summary>
    /// <returns>file path and content as string</returns>
    internal (string filePath, string fileContent) Load()
    {
        if (!File.Exists(FeatureFilePath))
            return (FeatureFilePath, null);

        var body = File.ReadAllText(FeatureFilePath);
        return (FeatureFilePath, body);
    }

    private string FeatureFilePath => field ??= GetFeaturesFilePath();

    private string GetFeaturesFilePath()
    {
        // folder with "features.json"
        var configurationsPath = globalConfiguration.ConfigFolder();

        // ensure that path to store files already exits
        Directory.CreateDirectory(configurationsPath);

        var featureFilePath = Path.Combine(configurationsPath, FeatureConstants.FeaturesJson);
        return featureFilePath;
    }

    /// <summary>
    /// Update existing features config in "features.json". 
    /// </summary>
    internal bool Save(string fileContent)
    {
        var l = Log.Fn<bool>($"fp={FeatureFilePath}");
        try
        {
            File.WriteAllText(FeatureFilePath, fileContent);
            return l.ReturnTrue("ok, file saved");
        }
        catch (Exception e)
        {
            l.Ex(e);
            return l.ReturnFalse($"save file failed:{e.Message}");
        }
    }
}