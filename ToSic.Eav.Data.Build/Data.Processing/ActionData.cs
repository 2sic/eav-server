namespace ToSic.Eav.Data.Processing;

public abstract record ActionData
{
    public virtual object? DataUntyped { get; init; }

    public DataPreprocessorDecision Decision
    {
        get => Exceptions.Any() ? DataPreprocessorDecision.Error : field;
        init;
    } = DataPreprocessorDecision.Continue;

    public string? LogMessage { get; init; }

    public string? ErrorMessage { get; init; }

    public List<Exception> Exceptions
    {
        get;
        init;
    } = [];

    public static ActionData<TNewData> Create<TNewData>(TNewData data) => new() { Data = data };

}
public record ActionData<TData>() : ActionData
{

    [SetsRequiredMembers]
    public ActionData(TData data): this()
        => Data = data;


    public override object? DataUntyped => Data;

    public required TData Data { get; init; }


};

/// <summary>
/// old name, leave in for @STV for now - should move to action data
/// </summary>
/// <typeparam name="TData"></typeparam>
public record DataProcessorResult<TData>: ActionData<TData>;
