namespace ToSic.Eav.Data.Processing;

public record DataProcessorResult<TData>(TData Data, DataPreprocessorDecision Decision, Exception? Exception = default);
