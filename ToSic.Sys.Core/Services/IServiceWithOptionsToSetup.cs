namespace ToSic.Sys.Services;
public interface IServiceWithOptionsToSetup<in TOptions>
{
    void SetOptions(TOptions options);
}
