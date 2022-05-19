using System;
using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Logging;

namespace ToSic.Eav.Plumbing.DI
{
    public class ServiceSwitcher<T>: HasLog, ILazyLike<T> where T : ISwitchableService
    {
        // TODO
        // - constructor with name
        // - add to global log history when regenerating incl. choice

        public ServiceSwitcher(IEnumerable<T> allServices) : base(LogNames.Eav + ".SrvSwt")
        {
            AllServices = allServices?.ToList();
        }

        public readonly List<T> AllServices;


        public T Value => _preferredService.Get(FindServiceInSwitcher);
        private readonly ValueGetOnce<T> _preferredService = new ValueGetOnce<T>();

        private T FindServiceInSwitcher()
        {
            var wrapLog = Log.Call2<T>();
            var all = AllServices;
            if (all == null || !all.Any())
                throw new ArgumentException(
                    $"There ware no services for the type '{typeof(T).FullName}', cannot find best option");

            foreach (var svc in all.OrderByDescending(s => s.Priority))
                if (svc.IsViable())
                    return wrapLog.Return(svc, $"Will keep '{svc.NameId}'");
                else Log.Add($"Service '{svc.NameId}' not viable");

            throw new ArgumentException($"No viable services found for type '{typeof(T).FullName}'");
        }


        public bool IsValueCreated => _preferredService != null;

        public T ByNameId(string nameId) => AllServices.Find(s => s.NameId.Equals(nameId));
    }
}
