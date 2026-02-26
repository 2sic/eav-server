namespace ToSic.Eav.Data.Processing;

public interface ILowCodeAction<in TContext, TDataIn, TDataOut>
{
    public Task<ActionData<TDataOut>> Run(TContext mainCtx, ActionData<TDataIn> result);

}
