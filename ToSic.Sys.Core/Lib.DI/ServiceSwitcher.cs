using ToSic.Lib.Helpers;
using ToSic.Lib.Services;
using static System.StringComparison;

namespace ToSic.Lib.DI;

[ShowApiWhenReleased(ShowApiMode.Never)]
public class ServiceSwitcher<T>(IEnumerable<T> allServices) : ServiceBase($"{LogScopes.Lib}.SrvSwt"), ILazyLike<T>
    where T : ISwitchableService
{
    // TODO
    // - add to global log history when regenerating incl. choice

    // ReSharper disable once ConditionalAccessQualifierIsNonNullableAccordingToAPIContract
    public readonly List<T> AllServices = allServices?.ToList() ?? [];


    public T Value => _preferredService.Get(FindServiceInSwitcher)!;
    private readonly GetOnce<T> _preferredService = new();

    private T FindServiceInSwitcher()
    {
        var l = Log.Fn<T>();

        if (!AllServices.Any())
            throw new ArgumentException(
                $"There ware no services for the type '{typeof(T).FullName}', cannot find best option");

        foreach (var svc in AllServices.OrderByDescending(s => s.Priority))
            if (svc.IsViable())
                return l.ReturnAndLog(svc, $"Will keep '{svc.NameId}'");
            else l.A($"Service '{svc.NameId}' not viable");

        throw new ArgumentException($"No viable services found for type '{typeof(T).FullName}'");
    }


    public bool IsValueCreated => _preferredService.IsValueCreated;

    public T? ByNameId(string nameId, bool insensitive = false)
        => AllServices.Find(s => s.NameId.Equals(nameId, insensitive ? InvariantCultureIgnoreCase : Ordinal));
}