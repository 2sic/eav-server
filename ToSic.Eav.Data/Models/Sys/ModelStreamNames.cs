using ToSic.Sys.Utils.Types;

namespace ToSic.Eav.Models.Sys;

public static class ModelStreamNames
{
    #region Stream Names WIP

    /// <summary>
    /// Get the stream names of the current type.
    /// </summary>
    /// <typeparam name="TCustom"></typeparam>
    /// <returns></returns>
    public static IList<string> GetStreamNameList<TCustom>() where TCustom : class
    {
        return StreamNames.Get<TCustom, ModelSpecsAttribute>(attribute =>
            ModelNameVariants.GetFromNameOrFromType(attribute?.Stream, typeof(TCustom)));
    }

    private static readonly TypeAttributeLookup<IList<string>> StreamNames = new();

    #endregion
    
}