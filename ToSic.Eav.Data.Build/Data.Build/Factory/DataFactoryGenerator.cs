namespace ToSic.Eav.Data.Build;

/// <summary>
/// Generator to get a data factory; will require options to get the true data factory.
/// </summary>
/// <param name="sp"></param>
[InternalApi_DoNotUse_MayChangeWithoutNotice]
public class DataFactoryGenerator(ISysServiceProvider sp) : Generator<IDataFactory, DataFactoryOptions>(sp);
