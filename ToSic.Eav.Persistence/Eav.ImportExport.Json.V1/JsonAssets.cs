namespace ToSic.Eav.ImportExport.Json.V1;

public class JsonAssets
{
    public JsonAsset Get(string realPath, string relativeName, string storage)
    {
        if (storage == null || !JsonAsset.Storages.Contains(storage))
            throw new ArgumentException($@"'{storage}' not a known value", nameof(storage));

        var name = Path.GetFileName(relativeName);
        var folder = Path.GetDirectoryName(relativeName);
        var ast = new JsonAsset
        {
            Name = name,
            Folder = folder,
            File = null,
            Storage = storage,
        };

        if (!File.Exists(realPath))
            return ast;

        // Is it binary? Check for 2 consecutive nulls..
        var content = File.ReadAllBytes(realPath);
        for (var i = 1; i < 512 && i < content.Length; i++)
            if (content[i] == 0x00 && content[i - 1] == 0x00)
                return ast with
                {
                    Encoding = JsonAsset.EncodingBase64,
                    File = Convert.ToBase64String(content),

                };

        // No? return text
        return ast with
        {
            File = File.ReadAllText(realPath),
        };
    }

    public bool Create(string? realPath, JsonAsset asset, bool overwrite = false)
    {
        if (string.IsNullOrWhiteSpace(realPath))
            return false;

        // 1. check if target file exists
        if (File.Exists(realPath) && !overwrite)
            return false;

            
        // try to restore it
        if (asset.Encoding == JsonAsset.EncodingBase64)
        {
            var contents = Convert.FromBase64String(asset.File ?? "");
            File.WriteAllBytes(realPath, contents);
            return true;
        }

        if (string.IsNullOrEmpty(asset.Encoding) || asset.Encoding == JsonAsset.EncodingNone)
        {
            File.WriteAllText(realPath, asset.File);
            return true;
        }

        return false;
    }
}