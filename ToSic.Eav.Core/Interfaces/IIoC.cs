using Microsoft.Practices.Unity;

namespace ToSic.Eav.Interfaces
{
    public interface IIoC
    {
        IUnityContainer ConfiguredContainer { get; }
    }
}
