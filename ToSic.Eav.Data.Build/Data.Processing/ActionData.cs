namespace ToSic.Eav.Data.Processing;

[ShowApiWhenReleased(ShowApiMode.Never)]
public abstract record ActionData
{
    public DataPreprocessorDecision Decision
    {
        get => Exceptions.Any() ? DataPreprocessorDecision.Error : field;
        init;
    } = DataPreprocessorDecision.Continue;

    public List<Exception> Exceptions
    {
        get;
        init;
    } = [];

    public static ActionData<TNewData> Create<TNewData>(TNewData data) => new() { Data = data };

}

[ShowApiWhenReleased(ShowApiMode.Never)]
public record ActionData<TData>() : ActionData
{
    [SetsRequiredMembers]
    public ActionData(TData data): this()
        => Data = data;

    public required TData Data { get; init; }
}
