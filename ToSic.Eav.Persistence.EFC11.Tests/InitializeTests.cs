﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToSic.Eav.Implementations.UserInformation;
using ToSic.Eav.Implementations.ValueConverter;

namespace ToSic.Eav.UnitTests
{
    [TestClass]
    class InitializeTests
    {
        static string conStr = @"Data Source=(local)\SQLEXPRESS;Initial Catalog=""2flex 2Sexy Content"";Integrated Security=True;";

        [AssemblyInitialize()]
        public static void AssemblyInit(TestContext context)
        {
            // new DependencyInjection().ConfigureDefaultMappings(Eav.Factory.Container);
            //ConfigureDependencyInjection();
            ConfigureEfcDi();
        }

        // note: keep in sync w/the Repository.Efc.Tests and Persistence.Efc.Tests
        private static void ConfigureEfcDi()
        {
            Configuration.SetConnectionString(conStr);
            Factory.ActivateNetCoreDi(sc =>
            {
                sc.AddTransient<IEavValueConverter, NeutralValueConverter>();//new InjectionConstructor());
                sc.AddTransient<IEavUserInformation, NeutralEavUserInformation>();//new InjectionConstructor());
                sc.AddTransient<IEavUserInformation, NeutralEavUserInformation>();//new InjectionConstructor());

                // these are only used in Repository.Efc.Tests
                //sc.AddTransient<XmlExporter, ToSxcXmlExporter>();//(new InjectionConstructor(0, 0, true, new string[0], new string[0]));
                //sc.AddTransient<IImportExportEnvironment, ImportExportEnvironmentMock>();

                new DependencyInjection().ConfigureNetCoreContainer(sc);
            });

        }


        //private static void ConfigureDependencyInjection()
        //{
        //    var conStr = @"Data Source=(local)\SQLEXPRESS;Initial Catalog=""2flex 2Sexy Content"";Integrated Security=True;";
        //    Configuration.SetConnectionString(conStr);
        //    Eav.Factory.ActivateNetCoreDi(sc =>
        //    {

        //    });

        //    //var container = Eav.Factory.CreateContainer();//.Container;
        //    //container.RegisterType<EavDbContext>(new HierarchicalLifetimeManager(), new InjectionFactory(
        //    //    obj =>
        //    //    {
        //    //        var opts = new DbContextOptionsBuilder<EavDbContext>();
        //    //        opts.UseSqlServer(conStr);
        //    //        return new EavDbContext(opts.Options);
        //    //    }));
        //}
    }
}
