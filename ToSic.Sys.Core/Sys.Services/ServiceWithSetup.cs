namespace ToSic.Sys.Services;

// #NoEditorBrowsableBecauseOfInheritance
//[ShowApiWhenReleased(ShowApiMode.Never)]


[InternalApi_DoNotUse_MayChangeWithoutNotice("Still WIP as of v22")]
public abstract class ServiceWithSetup<TOptions>(string logName, NoParamOrder npo = default, object[]? connect = default)
    : ServiceBase(logName, npo, connect: connect),
        IHasOptions<TOptions>,
        IServiceWithSetup<TOptions>
        where TOptions : class //, new()
{
    /// <inheritdoc />
    [field: AllowNull, MaybeNull]
    public virtual TOptions MyOptions
    {
        get => field ??= GetDefaultOptions();
        private set;
    }

    public virtual void Setup(TOptions options)
        => MyOptions = options;

    /// <summary>
    /// Method to generate new / default options.
    /// This should usually not be called, but it might;
    /// so if your service can work without valid options (or accesses options before setup), this would trigger.
    ///
    /// This could also be triggered, if the service was created without the correct Generator.
    /// Note that it's always an option to just throw an exception.
    /// 
    /// You must override this to provide your own default options.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="ServiceOptionsRequiredException"></exception>
    protected virtual TOptions GetDefaultOptions()
        => throw new ServiceOptionsRequiredException();
}
