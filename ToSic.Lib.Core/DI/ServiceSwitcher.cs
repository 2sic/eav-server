using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Lib.Helpers;
using ToSic.Lib.Logging;
using ToSic.Lib.Services;

namespace ToSic.Lib.DI
{
    public class ServiceSwitcher<T>: ServiceBase, ILazyLike<T> where T : ISwitchableService
    {
        // TODO
        // - add to global log history when regenerating incl. choice

        public ServiceSwitcher(IEnumerable<T> allServices) : base($"{LogScopes.Lib}.SrvSwt")
        {
            AllServices = allServices?.ToList();
        }

        public readonly List<T> AllServices;


        public T Value => _preferredService.Get(FindServiceInSwitcher);
        private readonly GetOnce<T> _preferredService = new GetOnce<T>();

        private T FindServiceInSwitcher() => Log.Func(l =>
        {
            var all = AllServices;
            if (all == null || !all.Any())
                throw new ArgumentException(
                    $"There ware no services for the type '{typeof(T).FullName}', cannot find best option");

            foreach (var svc in all.OrderByDescending(s => s.Priority))
                if (svc.IsViable())
                    return (svc, $"Will keep '{svc.NameId}'");
                else l.A($"Service '{svc.NameId}' not viable");

            throw new ArgumentException($"No viable services found for type '{typeof(T).FullName}'");
        });


        public bool IsValueCreated => _preferredService.IsValueCreated;

        public T ByNameId(string nameId) => AllServices.Find(s => s.NameId.Equals(nameId));
    }
}
