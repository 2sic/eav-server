namespace ToSic.Eav.Models;

[Flags]
[WorkInProgressApi("WIP v22")]
public enum ModelNullHandling
{
    ///// <summary>
    ///// Represents an undefined state.
    ///// Will be treated as Default, but can be used to detect if the caller explicitly set it or not.
    ///// </summary>
    //Undefined = 0,

    ///// <summary>
    ///// If original data is null, return null.
    ///// This is the default behavior.
    ///// </summary>
    //DataNullAsNull = 1 << 0,

    ///// <summary>
    ///// If original data is null, throw an exception.
    ///// </summary>
    //DataNullThrows = 1 << 1,

    ///// <summary>
    ///// If original data is null, try to return a model, unless the model says otherwise.
    ///// </summary>
    //DataNullTryConvert = 1 << 2,

    ///// <summary>
    ///// If original data is null, try to return a model, unless the model says otherwise - in which case throw.
    ///// </summary>
    //DataNullTryConvertOrThrow = 1 << 3,

    ///// <summary>
    ///// If original data is null, force return a model, even if the model may not be able to handle it.
    ///// This is a very aggressive option and should only be used if you are sure that the model can handle null sources, or if you want to force it to do so for testing purposes.
    ///// </summary>
    //DataNullForceConvert = 1 << 4,

    // TODO: THIS is not implemented anywhere - but not sure if it should; ignore for now
    ///// <summary>
    ///// If the list is null, return an empty list anyhow.
    ///// This is only meant for list conversions, and does not affect single item conversions.
    ///// </summary>
    //ListNullAsEmpty = 1 << 5,

    // TODO: This was handled in 2 places and never used, ignore for now
    ///// <summary>
    ///// If the list is null, return an empty list anyhow.
    ///// This is only meant for list conversions, and does not affect single item conversions.
    ///// </summary>
    //ListNullThrows = 1 << 6,


    ///// <summary>
    ///// Return null if the model reports not being able to handle the data given to it.
    ///// This is the default.
    ///// </summary>
    //ModelNullAsNull = 1 << 7,

    //ModelNullSkip = 1 << 8,

    //ModelNullThrows = 1 << 9,

    //ModelNullAsModel = 1 << 10,



    // Only added to presets, never evaluated, ignore for now
    //TypeCheckAsNull = 1 << 11,

    // Never used
    //TypeCheckFilter = 1 << 12,

    // Only added to presets, never evaluated, ignore for now
    //TypeCheckIgnore = 1 << 13,

    // Never used
    //TypeCheckThrow = 1 << 14,

    ///// <summary>
    ///// Defaults
    ///// 
    ///// * null-lists will return an empty list
    ///// * null-data will return null
    ///// * null-models will return null
    ///// * in list scenarios, null-models will be filtered out
    ///// </summary>
    //Default = //ListNullAsEmpty | 
    //          DataNullAsNull |
    //          //TypeCheckAsNull |
    //          ToModelOptions.ModelNullHandling.ModelNullAsNull |
    //          ToModelOptions.ModelNullHandling.ModelNullSkip,

    //PreferNull = // ListNullAsEmpty |
    //             DataNullAsNull |
    //             //TypeCheckAsNull |
    //             ToModelOptions.ModelNullHandling.ModelNullAsNull |
    //             ToModelOptions.ModelNullHandling.ModelNullSkip,

    //PreferModel = // ListNullAsEmpty |
    //              DataNullForceConvert |
    //              //TypeCheckIgnore |
    //              ToModelOptions.ModelNullHandling.ModelNullAsModel,
}
