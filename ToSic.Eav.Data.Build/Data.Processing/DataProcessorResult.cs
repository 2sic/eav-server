namespace ToSic.Eav.Data.Processing;

public record DataProcessorResult<TData>
{
    public required TData Data { get; init; }

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
};
