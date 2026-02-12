namespace ToSic.Eav.Data.Processing;

public record DataProcessorResult<TData>(TData Data, Exception? Exception = default);
