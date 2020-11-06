using System;
using System.Collections.Generic;
using ToSic.Eav.Documentation;
using ToSic.Eav.Plumbing;
using ToSic.Eav.Run;

namespace ToSic.Eav.Apps.Run
{
    public class InstanceContext: IInstanceContext
    {
        public InstanceContext(ISite site, IPage page, IContainer container, IUser user, IServiceProvider serviceProvider)
        {
            Tenant = site;
            Page = page;
            Container = container;
            User = user;
            ServiceProvider = serviceProvider ?? throw new Exception("Context didn't receive service provider, but this is absolutely necessary.");
        }


        public ISite Tenant { get; }
        public IPage Page { get; protected set; }
        public IContainer Container { get; }
        public IUser User { get; }
        public IServiceProvider ServiceProvider { get; }

        public IInstanceContext Clone(ISite site = null, IPage page = null, IContainer container = null, IUser user = null) 
            => new InstanceContext(site ?? Tenant, page ?? Page, container ?? Container, user ?? User, ServiceProvider);

        #region Parameters / URL Parameters

        //public List<KeyValuePair<string, string>> Parameters
        //{
        //    get => _parameters ?? (_parameters = GetQueryStringKvp());
        //    set => _parameters = value;
        //}
        //private List<KeyValuePair<string, string>> _parameters;

        //protected abstract List<KeyValuePair<string, string>> GetQueryStringKvp();

        #endregion
    }
}
