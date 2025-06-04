namespace ToSic.Eav.Work;

internal interface IWorkTask<out T> where T : class, IWorkSpecs
{
    T Specs { get; }
}