namespace ToSic.Sys.Services;
internal interface IServiceWithOptionsToSetup<in TOptions>
{
    //internal TOptions Options { set; }
    internal void SetOptions(TOptions options);
}
