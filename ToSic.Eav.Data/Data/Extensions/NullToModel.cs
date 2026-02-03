namespace ToSic.Eav.Data;

[Flags]
public enum NullToModel
{
    /// <summary>
    /// Represents an undefined state.
    /// Will be treated as Default, but can be used to detect if the caller explicitly set it or not.
    /// </summary>
    Undefined = 0,

    /// <summary>
    /// If original data is null, return null.
    /// This is the default.
    /// </summary>
    DataAsNull = 1 << 0,

    /// <summary>
    /// If original data is null, throw an exception.
    /// </summary>
    DataAsThrow = 1 << 1,

    /// <summary>
    /// If original data is null, try to return a model, unless the model says otherwise.
    /// </summary>
    DataAsModelTry = 1 << 2,

    /// <summary>
    /// If original data is null, try to return a model, unless the model says otherwise - in which case throw.
    /// </summary>
    DataAsModelOrThrow = 1 << 3,

    /// <summary>
    /// If original data is null, force return a model, even if the model may not be able to handle it.
    /// This is a very aggressive option and should only be used if you are sure that the model can handle null sources, or if you want to force it to do so for testing purposes.
    /// </summary>
    DataAsModelForce = 1 << 4,


    /// <summary>
    /// If the list is null, return an empty list anyhow.
    /// This is only meant for list conversions, and does not affect single item conversions.
    /// </summary>
    ListAsEmpty = 1 << 5,

    /// <summary>
    /// If the list is null, return an empty list anyhow.
    /// This is only meant for list conversions, and does not affect single item conversions.
    /// </summary>
    ListAsThrow = 1 << 6,


    /// <summary>
    /// Return null if the model reports not being able to handle the data given to it.
    /// This is the default.
    /// </summary>
    ModelAsNull = 1 << 7,

    ModelAsSkip = 1 << 8,

    ModelAsThrow = 1 << 9,

    ModelAsModelForce = 1 << 10,



    TypeCheckAsNull = 1 << 11,

    TypeCheckFilter = 1 << 12,

    TypeCheckIgnore = 1 << 13,

    TypeCheckThrow = 1 << 14,

    /// <summary>
    /// Defaults
    /// 
    /// * null-lists will return an empty list
    /// * null-data will return null
    /// * null-models will return null
    /// * in list scenarios, null-models will be filtered out
    /// </summary>
    Default = ListAsEmpty
              | DataAsNull
              | TypeCheckAsNull
              | ModelAsNull
              | ModelAsSkip,

    PreferNull = ListAsEmpty
               | DataAsNull
               | TypeCheckAsNull
               | ModelAsNull
               | ModelAsSkip,

    PreferModelTry = ListAsEmpty
               | DataAsModelTry
               | TypeCheckIgnore
               | ModelAsNull,

    PreferModelForce = ListAsEmpty
               | DataAsModelForce
               | TypeCheckIgnore
               | ModelAsModelForce,
}
