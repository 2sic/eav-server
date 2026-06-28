namespace ToSic.Eav.Data.Processing;

[ShowApiWhenReleased(ShowApiMode.Never)]
public interface ILowCodeAction<TDataIn, TDataOut>
{
    public Task<ActionData<TDataOut>> Run(LowCodeActionContext mainCtx, ActionData<TDataIn> result);

}
